using MelonLoader;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MelonFearTrainer
{
    public class FearTrainer : MelonMod
    {
        private enum ModCategory
        {
            Player,
            Utility
        }

        private class Setting
        {
            public string Name;
            public bool IsPressed;
        }

        private Vector3 _currentPosition;

        private readonly Setting[] _playerSettings =
        {
            new Setting { Name = "Speed Hack", IsPressed = false },
            new Setting { Name = "Set Into Sofa", IsPressed = false }
        };

        private readonly Setting[] _utilitySettings =
        {
            new Setting { Name = "Load Day Scene", IsPressed = false },
            new Setting { Name = "Load Dark Scene", IsPressed = false },
            new Setting { Name = "Change Food Cook State", IsPressed = false },
            new Setting { Name = "Toggle Triggers", IsPressed = false }
        };

        private bool _showMenu;

        private GUIStyle _labelStyle;
        private GUIStyle _buttonStyle;
        private GUIStyle _textAreaStyle;

        private readonly Color _buttonBgColor = Color.black;
        private readonly Color _buttonActiveBgColor = Color.red;
        private readonly Color _buttonHoverBgColor = Color.gray;
        private readonly Color _textColor = new Color(0.8f, 0.8f, 0.8f);

        private FirstPersonAIO _playerController;

        public override void OnGUI()
        {
            OnPracticeGUI();
            if (_showMenu)
            {
                DrawModMenu();
                Cursor.lockState = CursorLockMode.Confined;
            }
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            OnPracticeUpdate();
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                ToggleMenu();
            }

            if (_showMenu)
            {
                ApplyChanges();
            }

            if (_playerController != null)
            {
                SupplyPosition(_playerController.transform.position);
            }
        }

        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            if (_playerController == null)
            {
                var go = GameObject.FindWithTag("Player");
                if (go != null)
                {
                    _playerController = go.GetComponent<FirstPersonAIO>();
                }
            }
        }

        private void InitializeStyles()
        {
            _labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = _textColor }
            };

            _buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleCenter,
                normal = { background = MakeTex(2, 2, _buttonBgColor) },
                active = { background = MakeTex(2, 2, _buttonActiveBgColor) },
                hover = { background = MakeTex(2, 2, _buttonHoverBgColor) },
            };

            _textAreaStyle = new GUIStyle(GUI.skin.textArea)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = _textColor },
                fixedWidth = 50
            };
        }

        private string _teleportPosX = "0";
        private string _teleportPosY = "0";
        private string _teleportPosZ = "0";

        private void DrawModMenu()
        {
            if (_labelStyle == null)
            {
                InitializeStyles();
            }

            Cursor.visible = true; // Show cursor when menu is open
            float windowWidth = 400f; // Width of the window
            float windowHeight = 400f; // Height of the window

            Rect windowRect = new Rect(Screen.width - windowWidth - 10f, 10f, windowWidth, windowHeight);

            GUILayout.Window(0, windowRect, id =>
            {
                GUILayout.BeginVertical();
                GUILayout.Label("Mod Menu", _labelStyle);

                DrawSettings(ModCategory.Player);
                DrawSettings(ModCategory.Utility);

                GUILayout.Label("Description:", _labelStyle);

                GUILayout.Label("Current Position: " + _currentPosition.ToString(), _labelStyle);

                GUILayout.Space(20);

                GUILayout.Label("Teleport Position", _labelStyle);
                GUILayout.BeginHorizontal();
                GUILayout.Label("X:", _labelStyle);
                _teleportPosX = GUILayout.TextField(_teleportPosX, 5, _textAreaStyle);
                GUILayout.Label("Y:", _labelStyle);
                _teleportPosY = GUILayout.TextField(_teleportPosY, 5, _textAreaStyle);
                GUILayout.Label("Z:", _labelStyle);
                _teleportPosZ = GUILayout.TextField(_teleportPosZ, 5, _textAreaStyle);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Teleport", _buttonStyle))
                {
                    TeleportToPosition(new Vector3(float.Parse(_teleportPosX), float.Parse(_teleportPosY),
                        float.Parse(_teleportPosZ)));
                }

                if (GUILayout.Button("Close", _buttonStyle))
                {
                    _showMenu = false;
                }

                GUILayout.EndHorizontal();

                GUILayout.EndVertical();
            }, "", GUIStyle.none);

            Cursor.visible = false;
        }

        private void SupplyPosition(Vector3 position)
        {
            _currentPosition = position;
        }

        private void TeleportToPosition(Vector3 position)
        {
            if (_playerController != null)
            {
                _playerController.transform.position = position;
            }

            MelonLogger.Msg($"Teleporting to position: {position}");
        }

        private void DrawSettings(ModCategory category)
        {
            GUILayout.Label(category.ToString(), _labelStyle);

            Setting[] settings = category == ModCategory.Player ? _playerSettings : _utilitySettings;

            foreach (var setting in settings)
            {
                DrawSetting(setting);
            }
        }

        private void DrawSetting(Setting setting)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(setting.Name, _buttonStyle))
            {
                MelonLogger.Msg("Toggled " + setting.Name + " to " + !setting.IsPressed);

                setting.IsPressed = !setting.IsPressed;
            }

            GUILayout.EndHorizontal();
        }

        private bool _triggersDrawn = false;
        private bool _speedChanged = false;

        private void ApplyChanges()
        {
            if (_playerSettings[0].IsPressed && !_speedChanged)
            {
                MelonLogger.Msg("Applying Speed hack");
                Time.timeScale = 2f;
                _speedChanged = true;
            }
            else if (_speedChanged && _playerSettings[0].IsPressed)
            {
                Time.timeScale = 1f;
                _speedChanged = false;
            }

            if (_playerSettings[1].IsPressed)
            {
                MelonLogger.Msg("Set Into Sofa enabled");

                GameObject sofa = GameObject.Find("Sofa_House");
                if (sofa != null)
                {
                    sofa.GetComponent<sofaSwitch>().StartCoroutine("SitOnSofa");
                }

                _playerSettings[1].IsPressed = false;
            }

            if (_utilitySettings[0].IsPressed)
            {
                MelonLogger.Msg("Load Day Scene enabled");
                SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
                SceneManager.LoadScene("Scene 1");

                _utilitySettings[0].IsPressed = false;
            }

            if (_utilitySettings[1].IsPressed)
            {
                MelonLogger.Msg("Load Dark Scene enabled");

                SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
                SceneManager.LoadScene("Scene 1 Dark");

                _utilitySettings[1].IsPressed = false;
            }

            if (_utilitySettings[2].IsPressed)
            {
                MelonLogger.Msg("Changed Food Cook State");

                GameObject food = GameObject.Find("food");
                if (food != null)
                {
                    var foodComp = food.GetComponent<food>();
                    foodComp.foodCooked = !foodComp.foodCooked;
                }

                _utilitySettings[2].IsPressed = false;
            }

            if (_utilitySettings[3].IsPressed)
            {
                if (!_triggersDrawn)
                {
                    var go = GameObject.Find("TRIGGERS");
                    if (go == null)
                    {
                        return;
                    }

                    _triggersDrawn = true;

                    var colliders = go.GetComponentsInChildren<BoxCollider>();
                    if (colliders == null)
                    {
                        return;
                    }

                    foreach (var collider in colliders)
                    {
                        if (collider == null || collider.gameObject == null)
                        {
                            continue;
                        }

                        LineRenderer lineRenderer = collider.gameObject.AddComponent<LineRenderer>();
                        if (lineRenderer == null)
                        {
                            continue;
                        }

                        var shader = Shader.Find("Sprites/Default");
                        if (shader == null)
                        {
                            continue;
                        }

                        lineRenderer.material = new Material(shader);
                        lineRenderer.startWidth = 0.01f;
                        lineRenderer.endWidth = 0.01f;
                        var randColor = Random.ColorHSV();
                        lineRenderer.startColor = randColor;
                        lineRenderer.endColor = randColor;

                        var cubeVertices = new[]
                        {
                            new Vector3(1, 1, 1), new Vector3(1, 1, -1), new Vector3(-1, 1, -1),
                            new Vector3(-1, 1, 1), new Vector3(1, 1, 1), new Vector3(1, -1, 1),
                            new Vector3(1, -1, -1), new Vector3(-1, 1, -1), new Vector3(-1, -1, 1),
                            new Vector3(1, -1, 1), new Vector3(1, 1, -1), new Vector3(1, -1, -1),
                            new Vector3(-1, 1, -1), new Vector3(-1, -1, -1), new Vector3(-1, 1, 1),
                            new Vector3(-1, -1, 1), new Vector3(1, 1, 1), new Vector3(-1, 1, -1),
                            new Vector3(-1, -1, -1), new Vector3(1, -1, -1)
                        };
                        lineRenderer.positionCount = 20;
                        lineRenderer.useWorldSpace = false;
                        lineRenderer.SetPositions(cubeVertices);
                    }
                }
            }
            else
            {
                if (_triggersDrawn)
                {
                    MelonLogger.Msg("Triggers are disabled");
                    var go = GameObject.Find("TRIGGERS");
                    _triggersDrawn = false;
                    if (go != null)
                    {
                        var colliders = go.GetComponentsInChildren<BoxCollider>();
                        foreach (var collider in colliders)
                        {
                            var lineRenderer = collider.gameObject.GetComponent<LineRenderer>();
                            if (lineRenderer != null)
                            {
                                Object.DestroyImmediate(lineRenderer);
                            }
                        }
                    }
                }
            }
        }

        private void ToggleMenu()
        {
            _showMenu = !_showMenu;
        }

        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i)
            {
                pix[i] = col;
            }

            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        public override void OnInitializeMelon()
        {
            base.OnInitializeMelon();

            MelonLogger.Msg("FearTrainer loaded!");
        }

        private class PracticeData
        {
            public bool AlarmClockClicked;
            public bool HomeworkDone;
            public bool FoodDone;
            public bool BedPlayerLoaded;
            public bool PlayerHiding;
        }

        private bool _practicePluginEnabled = false;
        private PracticeData _practiceData = new PracticeData();

        private horrorEventsHandler _horrorEventsHandler;
        
        private void SetInstanceValues()
        {
            if (!_practiceData.AlarmClockClicked)
            {
                var bedPlayer = GameObject.Find("Bed Player");
                if (bedPlayer != null)
                {
                    if (!_practiceData.BedPlayerLoaded)
                    {
                        _practiceData.BedPlayerLoaded = true;
                    }

                    bool alarmClockClicked = bedPlayer.GetComponentInChildren<Camera>()
                        .GetComponent<sleepCamRaycast>()
                        .clicked;
                    _practiceData.AlarmClockClicked = alarmClockClicked;
                }
            }

            if (!_practiceData.HomeworkDone)
            {
                bool homeworkDone = PlayerPrefs.GetInt("HomeworkDone") == 1;
                _practiceData.HomeworkDone = homeworkDone;
            }

            if (!_practiceData.FoodDone)
            {
                bool foodDone = PlayerPrefs.GetInt("FoodDone") == 1;
                _practiceData.FoodDone = foodDone;
            }

            if (_horrorEventsHandler != null)
            {
                _practiceData.PlayerHiding = _horrorEventsHandler.playerInHideRoom;
            }
            
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            base.OnSceneWasLoaded(buildIndex, sceneName);
            if (sceneName == "Scene 1 Dark")
            {
                _horrorEventsHandler = GameObject.Find("GM").GetComponent<horrorEventsHandler>();
            }
        }

        public void OnPracticeUpdate()
        {
            if (Input.GetKeyDown(KeyCode.N))
            {
                _practicePluginEnabled = !_practicePluginEnabled;
                MelonLogger.Log("Speedrun Practice Plugin " + (_practicePluginEnabled ? "Enabled" : "Disabled"));
            }

            if (_practicePluginEnabled)
            {
                SetInstanceValues();
            }
        }

        public void OnPracticeGUI()
        {
            if (_practicePluginEnabled)
            {
                GUIStyle style = new GUIStyle(GUI.skin.label);
                Rect guiArea = new Rect(0, 0, Screen.width, Screen.height);
                
                GUILayout.BeginArea(guiArea);
                GUILayout.BeginVertical();

                GUILayout.FlexibleSpace(); // Pushes the following content to the center

                style.normal.textColor = _practiceData.AlarmClockClicked ? Color.green : Color.red;
                GUILayout.Label("Alarm Clock Clicked: " + _practiceData.AlarmClockClicked, style);

                style.normal.textColor = _practiceData.HomeworkDone ? Color.green : Color.red;
                GUILayout.Label("Homework Done: " + _practiceData.HomeworkDone, style);

                style.normal.textColor = _practiceData.FoodDone ? Color.green : Color.red;
                GUILayout.Label("Food Done: " + _practiceData.FoodDone, style);

                style.normal.textColor = _practiceData.BedPlayerLoaded ? Color.green : Color.red;
                GUILayout.Label("Bed Player Loaded: " + _practiceData.BedPlayerLoaded, style);

                style.normal.textColor = _practiceData.PlayerHiding ? Color.green : Color.red;
                GUILayout.Label("Player Hiding: " + _practiceData.PlayerHiding, style);

                GUILayout.FlexibleSpace(); // Pushes the above content to the center

                GUILayout.EndVertical();
                GUILayout.EndArea();
            }
        }
    }
}