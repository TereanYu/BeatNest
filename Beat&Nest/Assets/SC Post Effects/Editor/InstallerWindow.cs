// SC Post Effects
// Staggart Creations
// http://staggart.xyz

#if UNITY_2018_1_OR_NEWER //Minimum version that supports a Package Manager installation and ShaderIncludePathAttribute attribute
#define PACKAGE_MANAGER
#else
#undef PACKAGE_MANAGER
#endif

using UnityEditor;
using UnityEngine;

namespace SCPE
{
    public class InstallerWindow : EditorWindow
    {
        //Window properties
        private static readonly int width = 450;
        private static readonly int height = 550;
        private Vector2 scrollPos;

        private static bool hasError = false;

        public enum Tab
        {
            Start,
            Install,
            Finish
        }

        public static Tab INSTALLATION_TAB
        {
            get { return (Tab)SessionState.GetInt("INSTALLATION_PROGRESS", 0); }
            set { SessionState.SetInt("INSTALLATION_PROGRESS", (int)value); }
        }

#if !SCPE || SCPE_DEV
        [MenuItem("Help/SC Post Effects Installer", false, 0)]
#endif
        public static void ShowWindow()
        {
            EditorWindow editorWindow = GetWindow(typeof(InstallerWindow), false, " Installer", true);

            editorWindow.titleContent.image = EditorGUIUtility.IconContent("_Popup").image;
            editorWindow.autoRepaintOnSceneChange = true;
            editorWindow.ShowAuxWindow();

            //Open somewhat in the center of the screen
            editorWindow.position = new Rect(Screen.width / 2, 175f, width, height);

            //Fixed size
            editorWindow.maxSize = new Vector2(width, height);
            editorWindow.minSize = new Vector2(width, height);

            Init();

            editorWindow.Show();
        }

        private static void Init()
        {
            Installer.Initialize();
            INSTALLATION_TAB = Tab.Start;
        }

        private void OnInspectorUpdate()
        {
            Repaint();
        }

        private void OnDisable()
        {
            //Incase installation fails halfway
            Installer.CURRENTLY_INSTALLING = false;
        }

        private void OnGUI()
        {
            if (INSTALLATION_TAB < 0) INSTALLATION_TAB = 0;

            if (EditorApplication.isCompiling)
            {
                this.ShowNotification(new GUIContent(" Compiling...", EditorGUIUtility.IconContent("cs Script Icon").image));
            }
            else
            {
                this.RemoveNotification();
            }

            //Header
            {
                if (SCPE_GUI.HeaderImg)
                {
                    Rect headerRect = new Rect(0, -5, width, SCPE_GUI.HeaderImg.height);
                    UnityEngine.GUI.DrawTexture(headerRect, SCPE_GUI.HeaderImg, ScaleMode.ScaleToFit);
                    GUILayout.Space(SCPE_GUI.HeaderImg.height - 10);
                }
                else
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("<b><size=24>SC Post Effects</size></b>\n<size=16>For Post Processing Stack</size>", SCPE_GUI.Header);
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    using (new EditorGUI.DisabledGroupScope((int)INSTALLATION_TAB != 0))
                    {
                        GUILayout.Toggle((INSTALLATION_TAB == 0), new GUIContent("Start"), SCPE_GUI.ProgressTab);
                    }
                    using (new EditorGUI.DisabledGroupScope((int)INSTALLATION_TAB != 1))
                    {
                        GUILayout.Toggle(((int)INSTALLATION_TAB == 1), "Installation", SCPE_GUI.ProgressTab);
                    }
                    using (new EditorGUI.DisabledGroupScope((int)INSTALLATION_TAB != 2))
                    {
                        GUILayout.Toggle(((int)INSTALLATION_TAB == 2), "Finish", SCPE_GUI.ProgressTab);
                    }
                }
            }

            GUILayout.Space(5f);

            //Body 
            Rect oRect = EditorGUILayout.GetControlRect();
            Rect bodyRect = new Rect(oRect.x + 10, 115, width - 20, height);

            GUILayout.BeginArea(bodyRect);
            {
                switch (INSTALLATION_TAB)
                {
                    case (Tab)0:
                        StartScreen();
                        break;
                    case (Tab)1:
                        InstallScreen();
                        break;
                    case (Tab)2:
                        FinalScreen();
                        break;
                }
            }
            GUILayout.EndArea();

            //Progress buttons


            Rect areaRect = new Rect(width / 2, height - 70, width / 2.2f, height - 25);
            GUILayout.BeginArea(areaRect);

            using (new EditorGUILayout.HorizontalScope())
            {
                //EditorGUILayout.PrefixLabel(" ");

                //Disable buttons when installing
                using (new EditorGUI.DisabledGroupScope(Installer.CURRENTLY_INSTALLING))
                {

                    //Disable back button on first screen
                    using (new EditorGUI.DisabledGroupScope(INSTALLATION_TAB == Tab.Start))
                    {
                        if (GUILayout.Button("Back", SCPE_GUI.ProgressButtonLeft))
                        {
                            INSTALLATION_TAB--;
                        }
                    }
                    using (new EditorGUI.DisabledGroupScope(hasError))
                    {
                        string btnLabel = "Next";
                        if (INSTALLATION_TAB == Tab.Start) btnLabel = "Next";
                        if (INSTALLATION_TAB == Tab.Install) btnLabel = "Install";
                        if (INSTALLATION_TAB == Tab.Install && Installer.IS_INSTALLED) btnLabel = "Finish";
                        if (INSTALLATION_TAB == Tab.Finish) btnLabel = "Close";

                        if (GUILayout.Button(btnLabel, SCPE_GUI.ProgressButtonRight))
                        {
                            if (INSTALLATION_TAB == Tab.Start)
                            {
                                INSTALLATION_TAB = Tab.Install;
                                return;
                            }

                            //When pressing install again
                            if (INSTALLATION_TAB == Tab.Install)
                            {
                                if (Installer.IS_INSTALLED == false)
                                {
                                    Installer.Install();
                                }
                                else
                                {
                                    INSTALLATION_TAB = Tab.Finish;
                                }

                                return;
                            }

                            if (INSTALLATION_TAB == Tab.Finish)
                            {
                                Installer.PostInstall();
                                this.Close();
                            }

                        }
                    }
                }
            }
            GUILayout.EndArea();

            //Footer
            areaRect = new Rect(width / 4, height - 30, width / 2.1f, height - 25);
            GUILayout.BeginArea(areaRect);
            EditorGUILayout.LabelField("- Staggart Creations -", SCPE_GUI.Footer);
            GUILayout.EndArea();

        }

        private void StartScreen()
        {
            EditorGUILayout.HelpBox("\nThis wizard will guide you through the installation of the SC Post Effects package\n\nPress \"Next\" to continue...\n", MessageType.Info);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Pre-install checks", SCPE_GUI.Header);

            EditorGUILayout.Space();

            using (new EditorGUILayout.VerticalScope(EditorStyles.textArea))
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
                //Folder
                /*
                {
#if !PACKAGE_MANAGER
                    string folderText = (Installer.IS_CORRECT_BASE_FOLDER) ? "Correct location" : "Outside \"PostProcessing/\"";
                    SCPE_GUI.Status folderStatus = (Installer.IS_CORRECT_BASE_FOLDER) ? SCPE_GUI.Status.Ok : SCPE_GUI.Status.Error;

                    SCPE_GUI.DrawStatusBox(new GUIContent("SC Post Effects folder", EditorGUIUtility.IconContent("FolderEmpty Icon").image), folderText, folderStatus);

                    if (!Installer.IS_CORRECT_BASE_FOLDER && (PostProcessingInstallation.IS_INSTALLED))
                    {
                        EditorGUILayout.HelpBox("Please move the SC Post Effects folder to where you've installed the Post Processing Stack", MessageType.Error);
                    }
#endif
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

                if (PostProcessingInstallation.IS_INSTALLED == false)
                {
#if PACKAGE_MANAGER
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField("       Installation source", EditorStyles.label);

                        if (GUILayout.Button(new GUIContent("GitHub"), PostProcessingInstallation.Config == PostProcessingInstallation.Configuration.GitHub ? SCPE_GUI.ToggleButtonLeftToggled : SCPE_GUI.ToggleButtonLeftNormal))
                        {
                            PostProcessingInstallation.Config = PostProcessingInstallation.Configuration.GitHub;
                        }
                        if (GUILayout.Button(new GUIContent("Package Manager"), PostProcessingInstallation.Config == PostProcessingInstallation.Configuration.PackageManager ? SCPE_GUI.ToggleButtonRightToggled : SCPE_GUI.ToggleButtonRightNormal))
                        {
                            PostProcessingInstallation.Config = PostProcessingInstallation.Configuration.PackageManager;
                        }
                    }
#else
                    PostProcessingInstallation.Config = PostProcessingInstallation.Configuration.GitHub;
#endif

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (PostProcessingInstallation.Config == PostProcessingInstallation.Configuration.GitHub)
                        {
                            if (SCPE_GUI.DrawActionBox(string.IsNullOrEmpty(PostProcessingInstallation.PACKAGE_PATH) ? "Download" : "Install", EditorGUIUtility.IconContent("BuildSettings.Standalone.Small").image))
                            {
                                //Download
                                if (PostProcessingInstallation.PACKAGE_PATH.Contains(".unitypackage") == false)
                                {
                                    Application.OpenURL(PostProcessingInstallation.PP_DOWNLOAD_URL);
                                    if (EditorUtility.DisplayDialog("Post Processing Stack download", "Once the file has been downloaded, locate the file path and install it", "Browse"))
                                    {
                                        PostProcessingInstallation.PACKAGE_PATH = EditorUtility.OpenFilePanel("Package download location", "", "unitypackage");
                                    }
                                }
                                //Install
                                else
                                {
                                    PostProcessingInstallation.InstallPackage();
                                }


                            }
                        }
                        else
                        {
                            if (SCPE_GUI.DrawActionBox("Install", EditorGUIUtility.IconContent("BuildSettings.Standalone.Small").image))
                            {
                                PostProcessingInstallation.InstallPackage();
                            }
                        }
                    }

                    EditorGUILayout.Space();

                    if (PostProcessingInstallation.Config == PostProcessingInstallation.Configuration.GitHub)
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayout.LabelField(new GUIContent(" Package location", EditorGUIUtility.IconContent("d_UnityLogo").image));

                            EditorGUILayout.TextField(PostProcessingInstallation.PACKAGE_PATH, SCPE_GUI.PathField, GUILayout.MaxWidth(180f));

                            if (GUILayout.Button("...", GUILayout.MaxWidth(30f)))
                            {
                                PostProcessingInstallation.PACKAGE_PATH = EditorUtility.OpenFilePanel("Package download location", "", "unitypackage");
                            }
                        }
                    }
                    else
                    {

                    }

                } //End if-installed

                EditorGUILayout.Space();
            }


            //Validate for errors before allowing to continue
            hasError = !UnityVersionCheck.COMPATIBLE;
            //hasError = !Installer.IS_CORRECT_BASE_FOLDER;
            hasError = (PostProcessingInstallation.IS_INSTALLED == false);
        }

        private void InstallScreen()
        {
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Options", SCPE_GUI.Header);

            EditorGUILayout.Space();

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    using (new EditorGUI.DisabledGroupScope(Installer.Demo.HAS_SCENE_PACKAGE == false))
                    {
                        //EditorGUILayout.LabelField("Install demo content");

                        Installer.Settings.installDemoContent = SCPE_GUI.BoolSwitchGUI.Draw(Installer.Settings.installDemoContent, "Demo scenes");
                        //Installer.Settings.installDemoContent = EditorGUILayout.Toggle(Installer.Settings.installDemoContent);
                    }

                    //When installed
                    if (Installer.Demo.SCENES_INSTALLED)
                    {
                        SCPE_GUI.DrawStatusBox(null, "Installed", SCPE_GUI.Status.Ok, false);
                    }
                    //Not installed and missing source
                    if (!Installer.Demo.SCENES_INSTALLED)
                    {
                        if (Installer.Demo.HAS_SCENE_PACKAGE == false) SCPE_GUI.DrawStatusBox(null, "Missing", SCPE_GUI.Status.Warning, false);
                    }

                }
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                if (Installer.Demo.HAS_SCENE_PACKAGE == true)
                {
                    EditorGUILayout.LabelField("Examples showing volume blending", EditorStyles.miniLabel);
                }
                if (Installer.Demo.HAS_SCENE_PACKAGE == false && Installer.Demo.SCENES_INSTALLED == false)
                {
                    EditorGUILayout.HelpBox("Also import the \"_DemoContents.unitypackage\" file to install.", MessageType.None);

                }
            }

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    using (new EditorGUI.DisabledGroupScope(Installer.Demo.HAS_SAMPLES_PACKAGE == false))
                    {
                        //EditorGUILayout.LabelField("Install demo content");

                        Installer.Settings.installSampleContent = SCPE_GUI.BoolSwitchGUI.Draw(Installer.Settings.installSampleContent, "Sample content");

                        if (Installer.Settings.installDemoContent) Installer.Settings.installSampleContent = true;
                        //Installer.Settings.installDemoContent = EditorGUILayout.Toggle(Installer.Settings.installDemoContent);
                    }

                    //When installed
                    if (Installer.Demo.SAMPLES_INSTALLED)
                    {
                        SCPE_GUI.DrawStatusBox(null, "Installed", SCPE_GUI.Status.Ok, false);
                    }
                    //Not installed and missing source
                    if (!Installer.Demo.SAMPLES_INSTALLED)
                    {
                        if (Installer.Demo.HAS_SAMPLES_PACKAGE == false) SCPE_GUI.DrawStatusBox(null, "Missing", SCPE_GUI.Status.Warning, false);
                    }

                }
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                if (Installer.Demo.HAS_SCENE_PACKAGE == true)
                {
                    EditorGUILayout.LabelField("Profiles and sample textures", EditorStyles.miniLabel);
                }
                if (Installer.Demo.HAS_SCENE_PACKAGE == false && Installer.Demo.SCENES_INSTALLED == false)
                {
                    EditorGUILayout.HelpBox("Also import the \"_Samples.unitypackage\" file to install.", MessageType.None);

                }
            }


            if (Installer.CURRENTLY_INSTALLING || Installer.IS_INSTALLED)
            {
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Log", SCPE_GUI.Header);

                EditorGUILayout.Space();
                using (new EditorGUILayout.VerticalScope(EditorStyles.textArea, UnityEngine.GUILayout.MaxHeight(150f)))
                {
                    scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

                    for (int i = 0; i < Installer.Log.NumItems; i++)
                    {
                        SCPE_GUI.DrawLogLine(Installer.Log.Read(i));
                    }

                    if (Installer.CURRENTLY_INSTALLING) scrollPos.y += 10f;

                    EditorGUILayout.EndScrollView();
                }

                if (Installer.IS_INSTALLED)
                {
                    //EditorGUILayout.HelpBox("Shaders have been configured for use with the " + PostProcessingInstallation.Config + " installation of the Post Processing Stack. You can reconfigure them through the Help window,", MessageType.None);
                }
            }

        }

        private void FinalScreen()
        {
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Installation complete", SCPE_GUI.Header);

            EditorGUILayout.Space();

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.Space();

                //Demo contents not installed, display option to delete package
                if (Installer.Settings.installDemoContent == false && Installer.Demo.HAS_SCENE_PACKAGE == true)
                {
                    using (new EditorGUILayout.HorizontalScope(EditorStyles.label))
                    {
                        Installer.Settings.deleteDemoContent = SCPE_GUI.BoolSwitchGUI.Draw(Installer.Settings.deleteDemoContent, "Delete demo package");

                        //EditorGUILayout.LabelField("Delete demo package");
                        //Installer.Settings.deleteDemoContent = EditorGUILayout.Toggle(Installer.Settings.deleteDemoContent);
                    }
                }
                using (new EditorGUILayout.HorizontalScope(EditorStyles.label))
                {
                    Installer.Settings.setupCurrentScene = SCPE_GUI.BoolSwitchGUI.Draw(Installer.Settings.setupCurrentScene, "Add post processing to current scene");

                    //EditorGUILayout.LabelField("Add post processing to current scene");
                    //Installer.Settings.setupCurrentScene = EditorGUILayout.Toggle(Installer.Settings.setupCurrentScene);
                }

                EditorGUILayout.Space();
            }

            EditorGUILayout.Space();

            using (new EditorGUILayout.VerticalScope())
            {
                //Support box

                EditorGUILayout.HelpBox("\nThe help window can be accessed through Window->SC Post Effects\n\nYou can use this to quickly add post processing to a scene, and to switch the Post Processing installation type (GitHub or Package Manager)\n", MessageType.Info);
            }


        }
    }

}
