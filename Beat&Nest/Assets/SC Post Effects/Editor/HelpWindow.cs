// SC Post Effects
// Staggart Creations
// http://staggart.xyz

using UnityEngine;
using UnityEditor;

namespace SCPE
{
    public class HelpWindow : EditorWindow, IHasCustomMenu
    {

#if SCPE
        [MenuItem("Help/SC Post Effects", false, 0)]
#endif
        public static void ExecuteMenuItem()
        {
            HelpWindow.ShowWindow();
        }

        //Window properties
        private static int width = 440;
        private static int height = 550;

        //Tabs
        private bool isTabSetup = true;
        private bool isTabInstallation = false;
        private bool isTabGettingStarted = false;
        private bool isTabSupport = false;

        // This interface implementation is automatically called by Unity.
        void IHasCustomMenu.AddItemsToMenu(GenericMenu menu)
        {
            menu.AddItem(new GUIContent("Reconfigure shader paths"), false, MenuItemShaderUpgrade);
        }

        private static void MenuItemShaderUpgrade()
        {
            Installer.ConfigureShaderPaths();
        }

        public static void ShowWindow()
        {
            EditorWindow editorWindow = GetWindow<HelpWindow>(false, "Help", true);
            //editorWindow.titleContent = new GUIContent(SCPE.ASSET_NAME);
            editorWindow.autoRepaintOnSceneChange = true;

            //Open somewhat in the center of the screen
            editorWindow.position = new Rect((Screen.width) / 2f, 175, width, height);

            //Fixed size
            editorWindow.maxSize = new Vector2(width, height);
            editorWindow.minSize = new Vector2(width, 200);

            Init();

            editorWindow.Show();

        }

        private void SetWindowHeight(float height)
        {
            this.maxSize = new Vector2(width, height);
            this.minSize = new Vector2(width, height);
        }

        //Store values in the volatile SessionState
        static void Init()
        {
            Installer.CheckRootFolder();
            PackageVersionCheck.CheckForUpdate();
            PostProcessingInstallation.CheckInstallation();
            PostProcessingInstallation.FindInstallationDir();
        }

        void OnGUI()
        {
            DrawHeader();

            GUILayout.Space(5);
            DrawTabs();
            GUILayout.Space(5);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            if (isTabSetup) DrawQuickSetup();

            if (isTabInstallation) DrawInstallation();

            if (isTabGettingStarted) DrawGettingStarted();

            if (isTabSupport) DrawSupport();

            //DrawActionButtons();

            EditorGUILayout.EndVertical();

            DrawFooter();
        }

        void DrawHeader()
        {
            SCPE_GUI.DrawWindowHeader(width, height);

            GUILayout.Label("Version: " + SCPE.INSTALLED_VERSION, SCPE_GUI.Footer);
        }

        void DrawTabs()
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Toggle(isTabSetup, "Quick Setup", SCPE_GUI.Tab))
            {
                isTabSetup = true;
                isTabInstallation = false;
                isTabGettingStarted = false;
                isTabSupport = false;
            }
            if (GUILayout.Toggle(isTabInstallation, "Installation", SCPE_GUI.Tab))
            {
                isTabSetup = false;
                isTabInstallation = true;
                isTabGettingStarted = false;
                isTabSupport = false;
            }

            if (GUILayout.Toggle(isTabGettingStarted, "Documentation", SCPE_GUI.Tab))
            {
                isTabSetup = false;
                isTabInstallation = false;
                isTabGettingStarted = true;
                isTabSupport = false;
            }

            if (GUILayout.Toggle(isTabSupport, "Support", SCPE_GUI.Tab))
            {
                isTabSetup = false;
                isTabInstallation = false;
                isTabGettingStarted = false;
                isTabSupport = true;
            }

            EditorGUILayout.EndHorizontal();
        }

        void DrawQuickSetup()
        {
            SetWindowHeight(375f);

            EditorGUILayout.HelpBox("\nThese actions will automatically configure your scene for use with the Post Processing Stack.\n", MessageType.Info);

            EditorGUILayout.Space();

            //Camera setup
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            {
                EditorGUILayout.LabelField("Setup component on active camera");
                if (GUILayout.Button("Execute")) AutoSetup.SetupCamera();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            //Volume setup
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            {
                EditorGUILayout.LabelField("Create a new global Post Processing volume");
                if (GUILayout.Button("Execute")) AutoSetup.SetupGlobalVolume();
            }
            EditorGUILayout.EndHorizontal();

        }

        void DrawInstallation()
        {
            SetWindowHeight(350f);

            using (new EditorGUILayout.VerticalScope())
            {
                EditorGUILayout.Space();

                //Package Version
                {
                    string versionText = null;
                    versionText = (PackageVersionCheck.IS_UPDATED) ? "Latest version" : "New version available";
                    SCPE_GUI.Status versionStatus;
                    versionStatus = (PackageVersionCheck.IS_UPDATED) ? SCPE_GUI.Status.Ok : SCPE_GUI.Status.Warning;

                    SCPE_GUI.DrawStatusBox(new GUIContent(SCPE.INSTALLED_VERSION, EditorGUIUtility.IconContent("cs Script Icon").image), versionText, versionStatus);
                }

                if (!PackageVersionCheck.IS_UPDATED)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (SCPE_GUI.DrawActionBox("Update", EditorGUIUtility.IconContent("BuildSettings.Standalone.Small").image))
                        {
                            SCPE.OpenStorePage();
                            this.Close();
                        }
                    }
                }

                //Unity Version
                {
                    string versionText = null;
                    versionText = (UnityVersionCheck.COMPATIBLE) ? "Compatible" : "Not compatible";
                    versionText = (UnityVersionCheck.UNTESTED) ? "Untested!" : versionText;
                    SCPE_GUI.Status versionStatus;
                    versionStatus = (UnityVersionCheck.COMPATIBLE) ? SCPE_GUI.Status.Ok : SCPE_GUI.Status.Error;
                    versionStatus = (UnityVersionCheck.UNTESTED) ? SCPE_GUI.Status.Warning : versionStatus;

                    SCPE_GUI.DrawStatusBox(new GUIContent("Unity " + UnityVersionCheck.UnityVersion, EditorGUIUtility.IconContent("UnityLogo").image), versionText, versionStatus);
                }
                /*
                //Folder
                {
                    if (PostProcessingInstallation.Config == PostProcessingInstallation.Configuration.GitHub)
                    {
                        string folderText = (Installer.IS_CORRECT_BASE_FOLDER) ? "Correct location" : "Outside \"PostProcessing/\"";
                        SCPE_GUI.Status folderStatus = (Installer.IS_CORRECT_BASE_FOLDER) ? SCPE_GUI.Status.Ok : SCPE_GUI.Status.Error;

                        SCPE_GUI.DrawStatusBox(new GUIContent("SC Post Effects folder", EditorGUIUtility.IconContent("FolderEmpty Icon").image), folderText, folderStatus);

                        if (!Installer.IS_CORRECT_BASE_FOLDER)
                        {
                            if (!Installer.IS_CORRECT_BASE_FOLDER)
                            {
                                EditorGUILayout.HelpBox("Please move the SC Post Effects folder to where you've installed the Post Processing Stack", MessageType.Error);
                            }
                        }
                    }

                }
                */
                //Color space
                {
                    string colorText = (UnityEditor.PlayerSettings.colorSpace == ColorSpace.Linear) ? "Linear" : "Linear is recommended";
                    SCPE_GUI.Status folderStatus = (UnityEditor.PlayerSettings.colorSpace == ColorSpace.Linear) ? SCPE_GUI.Status.Ok : SCPE_GUI.Status.Warning;

                    SCPE_GUI.DrawStatusBox(new GUIContent("Color space", EditorGUIUtility.IconContent("d_PreTextureRGB").image), colorText, folderStatus);
                }

                //Post Processing Stack
                string ppsText = (PostProcessingInstallation.IS_INSTALLED) ? (PostProcessingInstallation.Config == PostProcessingInstallation.Configuration.GitHub) ? "Installed (GitHub)" : "Installed (Package Manager)" : "Not installed";
                SCPE_GUI.Status ppsStatus = (PostProcessingInstallation.IS_INSTALLED) ? SCPE_GUI.Status.Ok : SCPE_GUI.Status.Error;

                string ppsLabel = "Post Processing Stack v2";
#if PACKAGE_MANAGER
                ppsLabel = "Post Processing";
#endif
                SCPE_GUI.DrawStatusBox(new GUIContent(ppsLabel, EditorGUIUtility.IconContent("Camera Gizmo").image), ppsText, ppsStatus);

                /*
                using (new EditorGUILayout.HorizontalScope(EditorStyles.label))
                {
                    EditorGUILayout.LabelField("Change shader configuration", EditorStyles.label);

                    if (GUILayout.Button(new GUIContent("GitHub"), SCPE_GUI.ToggleButtonLeftNormal))
                    {
                        Installer.ConfigureShaderPaths(PostProcessingInstallation.Configuration.GitHub);
                    }
                    if (GUILayout.Button(new GUIContent("Package Manager"), SCPE_GUI.ToggleButtonRightNormal))
                    {
                        Installer.ConfigureShaderPaths(PostProcessingInstallation.Configuration.PackageManager);
                    }
                }
                */
            }

        }

        void DrawGettingStarted()
        {
            SetWindowHeight(335);

            EditorGUILayout.HelpBox("Please view the documentation for further details about this package and its workings.", MessageType.Info);

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("<b><size=12>Documentation</size></b>\n<i>Usage instructions</i>", SCPE_GUI.Button))
                {
                    Application.OpenURL(SCPE.DOC_URL);
                }
                if (GUILayout.Button("<b><size=12>Effect details</size></b>\n<i>View effect examples</i>", SCPE_GUI.Button))
                {
                    Application.OpenURL(SCPE.DOC_URL + "#effects");
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        void DrawSupport()
        {
            SetWindowHeight(350f);

            EditorGUILayout.HelpBox("If you have any questions, or ran into issues, please get in touch.\n\nThis package is still in its Beta stage, feedback is greatly appreciated and will help improve it!", MessageType.Info);

            EditorGUILayout.Space();

            //Buttons box
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("<b><size=12>Email</size></b>\n<i>Contact</i>", SCPE_GUI.Button))
                {
                    Application.OpenURL("mailto:contact@staggart.xyz");
                }
                if (GUILayout.Button("<b><size=12>Twitter</size></b>\n<i>Follow developments</i>", SCPE_GUI.Button))
                {
                    Application.OpenURL("https://twitter.com/search?q=staggart%20creations");
                }
                if (GUILayout.Button("<b><size=12>Forum</size></b>\n<i>Join the discussion</i>", SCPE_GUI.Button))
                {
                    Application.OpenURL(SCPE.FORUM_URL);
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        //TODO: Implement after Beta
        private void DrawActionButtons()
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("<size=12>Rate</size>", SCPE_GUI.Button)) SCPE.OpenStorePage();

            if (GUILayout.Button("<size=12>Review</size>", SCPE_GUI.Button)) SCPE.OpenStorePage();

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }

        private void DrawFooter()
        {
            EditorGUILayout.LabelField("", UnityEngine.GUI.skin.horizontalSlider);
            EditorGUILayout.Space();
            GUILayout.Label("- Staggart Creations -", SCPE_GUI.Footer);
        }

        #region Styles
        private static GUIStyle _Header;
        public static GUIStyle Header
        {
            get
            {
                if (_Header == null)
                {
                    _Header = new GUIStyle(UnityEngine.GUI.skin.label)
                    {
                        richText = true,
                        alignment = TextAnchor.MiddleCenter,
                        wordWrap = true,
                        fontSize = 18,
                        fontStyle = FontStyle.Bold
                    };
                }

                return _Header;
            }
        }
        #endregion //Stylies

    }//SCPE_Window Class
}
