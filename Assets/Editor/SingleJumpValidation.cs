using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;

public static class SingleJumpValidation
{
    [Serializable] private sealed class TestResult
    {
        public string name;
        public bool passed;
        public string detail;
    }

    [Serializable] private sealed class Report
    {
        public string checkedAtUtc;
        public List<TestResult> tests = new List<TestResult>();
        public int passed;
        public int failed;
        public int testLogErrors;
        public string buildResult = "NotRun";
        public string buildPath;
        public int buildErrors;
    }

    private sealed class Fixture : IDisposable
    {
        public GameObject actor;
        public GameObject ground;
        public PlayerController player;
        public PhysicsCheck check;
        public Rigidbody2D body;
        public PlayAudioEventSO audioEvent;
        public int sounds;

        public Fixture()
        {
            ground = new GameObject("ValidationGround");
            ground.layer = 31;
            ground.transform.position = new Vector3(0f, -0.5f, 0f);
            ground.AddComponent<BoxCollider2D>().size = new Vector2(12f, 1f);
            actor = new GameObject("ValidationPlayer");
            actor.layer = 30;
            actor.transform.position = new Vector3(0f, 0.92f, 0f);
            body = actor.AddComponent<Rigidbody2D>();
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            actor.AddComponent<CapsuleCollider2D>().size = new Vector2(0.8f, 1.8f);
            check = actor.AddComponent<PhysicsCheck>();
            check.manual = true;
            check.bottomOffset = new Vector2(0f, -0.9f);
            check.checkRadius = 0.08f;
            check.groundLayer = 1 << 31;
            actor.AddComponent<PlayerAnimation>();
            AudioDefination audio = actor.AddComponent<AudioDefination>();
            audioEvent = ScriptableObject.CreateInstance<PlayAudioEventSO>();
            audioEvent.OnEventRaised += _ => sounds++;
            audio.palyAudioEvent = audioEvent;
            player = actor.AddComponent<PlayerController>();
            player.jumpForce = 16f;
            if (player.inputControl == null) Call(player, "Awake");
            Physics2D.SyncTransforms();
            for (int i = 0; i < 12; i++) Step();
        }

        public void Press() { Call(player, "Jump", default(InputAction.CallbackContext)); }
        public void Step()
        {
            Call(player, "FixedUpdate");
            Require(Physics2D.Simulate(0.02f), "Physics did not advance.");
            check.Check();
        }
        public bool Supported()
        {
            check.Check();
            return (bool)Call(player, "HasGroundSupport");
        }
        public void MoveTo(Vector2 position, Vector2 velocity)
        {
            body.position = position;
            actor.transform.position = new Vector3(position.x, position.y, 0f);
            body.velocity = velocity;
            Physics2D.SyncTransforms();
            Physics2D.Simulate(0.02f);
            check.Check();
        }
        public void Dispose()
        {
            PlayerInputControl control = player == null ? null : player.inputControl;
            if (control != null) control.Disable();
            if (actor != null) UnityEngine.Object.DestroyImmediate(actor);
            if (control != null) UnityEngine.Object.DestroyImmediate(control.asset);
            if (ground != null) UnityEngine.Object.DestroyImmediate(ground);
            if (audioEvent != null) UnityEngine.Object.DestroyImmediate(audioEvent);
        }
    }

    // Batch-only: opens an empty scene without saving project scenes.
    public static void RunBatch()
    {
        if (!Application.isBatchMode)
            throw new InvalidOperationException("Use a separate Unity batch session.");
        Report report = new Report
        {
            checkedAtUtc = DateTime.UtcNow.ToString("o"),
            buildPath = Argument("-singleJumpBuildPath")
        };
        string resultsPath = Argument("-singleJumpResults");
        SimulationMode2D oldMode = Physics2D.simulationMode;
        Vector2 oldGravity = Physics2D.gravity;
        int exitCode = 1;
        Application.LogCallback errorListener = (condition, trace, type) =>
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
                report.testLogErrors++;
        };
        try
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Physics2D.simulationMode = SimulationMode2D.Script;
            Physics2D.gravity = new Vector2(0f, -9.81f);
            Application.logMessageReceived += errorListener;
            Check(report, "Grounded jump and audio", f =>
            {
                f.Press();
                Require(f.sounds == 0, "Audio ran before the physics step.");
                f.Step();
                Require(f.body.velocity.y > 10f && f.sounds == 1, "Expected one jump and one sound.");
            });
            Check(report, "Repeated inputs before physics", f =>
            {
                for (int i = 0; i < 10; i++) f.Press();
                f.Step();
                Require(f.sounds == 1 && f.body.velocity.y < 17f, "Jump impulses stacked.");
            });
            Check(report, "Ascending airborne input", f =>
            {
                f.Press(); f.Step();
                float before = f.body.velocity.y;
                for (int i = 0; i < 8; i++) f.Press();
                f.Step();
                Require(f.sounds == 1 && f.body.velocity.y < before, "Accepted an airborne jump.");
            });
            Check(report, "Descending airborne input", f =>
            {
                f.MoveTo(new Vector2(0f, 4f), new Vector2(0f, -2f));
                f.Press(); f.Step();
                Require(f.sounds == 0 && f.body.velocity.y < 0f, "Falling actor jumped.");
            });
            Check(report, "Stale ground flag", f =>
            {
                f.MoveTo(new Vector2(0f, 4f), Vector2.zero);
                f.check.isGround = true;
                f.Press(); f.Step();
                Require(f.sounds == 0, "Stale ground flag allowed a jump.");
            });
            Check(report, "Walking off an edge", f =>
            {
                f.MoveTo(new Vector2(15f, 0.9f), Vector2.zero);
                f.Press(); f.Step();
                Require(f.sounds == 0, "Accepted a walk-off jump.");
            });
            foreach (string flag in new[] { "isAttack", "isHurt", "isDead" })
            {
                string captured = flag;
                Check(report, "Reject input during " + flag, f =>
                {
                    typeof(PlayerController).GetField(captured).SetValue(f.player, true);
                    f.Press(); f.Step();
                    Require(f.sounds == 0, "State restriction was ignored.");
                });
                Check(report, "Cancel queued input on " + flag, f =>
                {
                    f.Press();
                    typeof(PlayerController).GetField(captured).SetValue(f.player, true);
                    f.Step();
                    Require(f.sounds == 0, "Queued input bypassed the changed state.");
                    typeof(PlayerController).GetField(captured).SetValue(f.player, false);
                    f.Step();
                    Require(f.sounds == 0, "Cancelled input was replayed.");
                });
            }
            Check(report, "Five jumps with landing reset", f =>
            {
                for (int cycle = 0; cycle < 5; cycle++)
                {
                    f.Press(); f.Step();
                    Require(f.sounds == cycle + 1, "Landing did not restore jumping.");
                    bool landed = false;
                    for (int i = 0; i < 250; i++)
                    {
                        f.Step();
                        if (f.Supported()) { f.Step(); landed = true; break; }
                    }
                    Require(landed, "Actor never landed.");
                }
            });
            Check(report, "Disable clears queued input", f =>
            {
                f.Press();
                Call(f.player, "OnDisable"); Call(f.player, "OnEnable");
                f.Step();
                Require(f.sounds == 0, "Disabled queued input was replayed.");
                f.Press(); f.Step();
                Require(f.sounds == 1, "Fresh grounded input should still work.");
            });
            Check(report, "Re-enable in air does not grant a jump", f =>
            {
                f.Press(); f.Step();
                Call(f.player, "OnDisable"); Call(f.player, "OnEnable");
                f.Press(); f.Step();
                Require(f.sounds == 1, "Re-enabling granted an airborne jump.");
            });
            Check(report, "Wall contact is not landing", f =>
            {
                f.MoveTo(new Vector2(0f, 4f), new Vector2(0f, -1f));
                GameObject wall = new GameObject("ValidationWall");
                try
                {
                    wall.layer = 31;
                    wall.transform.position = new Vector3(0.39f, 4f, 0f);
                    wall.AddComponent<BoxCollider2D>().size = new Vector2(0.2f, 5f);
                    Physics2D.SyncTransforms();
                    f.Step(); f.Press(); f.Step();
                    Require(f.sounds == 0, "A wall was treated as floor support.");
                }
                finally { UnityEngine.Object.DestroyImmediate(wall); }
            });
            Check(report, "Low jump does not reset near floor", f =>
            {
                f.player.jumpForce = 0.9f;
                f.Press(); f.Step();
                bool airborne = false;
                for (int i = 0; i < 20; i++)
                {
                    if (!f.Supported()) { airborne = true; f.Press(); }
                    else if (airborne) break;
                    f.Step();
                    Require(f.sounds == 1, "Near-floor sensor restored an airborne jump.");
                }
                Require(airborne, "Low jump never left floor contact.");
            });
            Application.logMessageReceived -= errorListener;
            if (report.testLogErrors > 0 && report.failed == 0)
            {
                report.failed++;
                report.tests.Add(new TestResult { name = "Engine log errors", passed = false,
                    detail = report.testLogErrors + " unexpected engine errors during tests." });
            }
            Physics2D.simulationMode = oldMode;
            Physics2D.gravity = oldGravity;
            if (report.failed == 0 && !string.IsNullOrEmpty(report.buildPath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(report.buildPath));
                BuildReport build = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = new[] { "Assets/Scenes/Persistent.unity" },
                    locationPathName = report.buildPath,
                    target = BuildTarget.StandaloneWindows64,
                    options = BuildOptions.Development
                });
                report.buildResult = build.summary.result.ToString();
                report.buildErrors = build.summary.totalErrors;
            }
            exitCode = report.failed == 0 &&
                (string.IsNullOrEmpty(report.buildPath) || report.buildResult == "Succeeded") ? 0 : 1;
        }
        catch (Exception error)
        {
            report.failed++;
            report.tests.Add(new TestResult { name = "Validation runner", passed = false, detail = error.ToString() });
            Debug.LogException(error);
        }
        finally
        {
            Application.logMessageReceived -= errorListener;
            Physics2D.simulationMode = oldMode;
            Physics2D.gravity = oldGravity;
            if (!string.IsNullOrEmpty(resultsPath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(resultsPath));
                File.WriteAllText(resultsPath, JsonUtility.ToJson(report, true));
            }
            Debug.Log("SingleJumpValidation: " + report.passed + " passed, " + report.failed +
                " failed; build=" + report.buildResult);
            EditorApplication.Exit(exitCode);
        }
    }

    private static void Check(Report report, string name, Action<Fixture> test)
    {
        try
        {
            using (Fixture fixture = new Fixture())
            {
                Require(fixture.Supported(), "Fixture must start on the floor.");
                test(fixture);
            }
            report.passed++;
            report.tests.Add(new TestResult { name = name, passed = true, detail = "Passed" });
        }
        catch (Exception error)
        {
            report.failed++;
            report.tests.Add(new TestResult { name = name, passed = false, detail = error.ToString() });
            Debug.LogError(name + ": " + error);
        }
    }
    private static object Call(object target, string method, params object[] args)
    {
        MethodInfo info = target.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic);
        if (info == null) throw new MissingMethodException(target.GetType().Name, method);
        return info.Invoke(target, args);
    }
    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
    private static string Argument(string name)
    {
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i + 1 < args.Length; i++)
            if (args[i] == name) return args[i + 1];
        return null;
    }
}
