// SC Post Effects
// Staggart Creations
// http://staggart.xyz

using System;
using UnityEditor;
using UnityEngine;

namespace SCPE
{
    public class SCPE_GUI : Editor
    {

        public static void DrawDocumentationHeader(string url)
        {
            GUILayout.Space(-18f);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(new GUIContent(HelpIcon, "Open documentation\n\nHover over a parameter\n to read its description"), DocButton))
                {
                    Application.OpenURL(url);
                }

            }
            GUILayout.Space(5f);

        }
        public enum Status
        {
            Ok,
            Warning,
            Error
        }

        public static string HEADER_IMG_PATH
        {
            get { return SessionState.GetString(SCPE.ASSET_ABRV + "_HEADER_IMG_PATH", string.Empty); }
            set { SessionState.SetString(SCPE.ASSET_ABRV + "_HEADER_IMG_PATH", value); }
        }

        public static void DrawStatusBox(GUIContent content, string status, SCPE_GUI.Status type, bool boxed = true)
        {

            using (new EditorGUILayout.HorizontalScope(EditorStyles.label))
            {
                if (content != null)
                {
                    content.text = "  " + content.text;
                    EditorGUILayout.LabelField(content, EditorStyles.label, GUILayout.MaxWidth(200f));
                }
                DrawStatusString(status, type, boxed);
            }

        }

        public static bool DrawActionBox(string text, Texture image = null)
        {
            text = " " + text;

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(" ");

                if (GUILayout.Button(new GUIContent(text, image), GUILayout.MaxWidth(200f)))
                {
                    return true;
                }

            }

            return false;
        }

        public static void DrawSwitchBox(GUIContent content, string textLeft, string textRight, bool inLeft, bool inRight, out bool outLeft, out bool outRight)
        {
            outLeft = inLeft;
            outRight = inRight;

            using (new EditorGUILayout.HorizontalScope(EditorStyles.label))
            {
                using (new EditorGUILayout.HorizontalScope(EditorStyles.label))
                {
                    if (content != null)
                    {
                        content.text = "  " + content.text;
                        EditorGUILayout.LabelField(content, EditorStyles.label);
                    }

                    if (GUILayout.Button(new GUIContent(textLeft), (outLeft) ? ToggleButtonLeftToggled : ToggleButtonLeftNormal))
                    {
                        outLeft = true;
                        outRight = !outLeft;
                    }
                    if (GUILayout.Button(new GUIContent(textRight), (outRight) ? ToggleButtonRightToggled : ToggleButtonRightNormal))
                    {
                        outRight = true;
                        outLeft = !outRight;
                    }
                }
            }
        }

        public static void DrawStatusString(string text, Status status, bool boxed = true)
        {
            GUIStyle guiStyle = EditorStyles.label;
            Color defaultTextColor = GUI.contentColor;

            //Personal skin
            if (EditorGUIUtility.isProSkin == false)
            {
                defaultTextColor = GUI.skin.customStyles[0].normal.textColor;
                guiStyle = new GUIStyle();

                GUI.skin.customStyles[0] = guiStyle;
            }


            //Grab icon and text color for status
            Texture icon = null;
            Color statusTextColor = Color.clear;

            StyleStatus(status, out statusTextColor, out icon);


            if (EditorGUIUtility.isProSkin == false)
            {
                GUI.skin.customStyles[0].normal.textColor = statusTextColor;
            }
            else
            {
                GUI.contentColor = statusTextColor;
            }

            if (boxed)
            {
                using (new EditorGUILayout.HorizontalScope(StatusBox))
                {
                    EditorGUILayout.LabelField(new GUIContent(" " + text, icon), guiStyle);
                }
            }
            else
            {
                EditorGUILayout.LabelField(new GUIContent(" " + text, icon), guiStyle);
            }


            if (EditorGUIUtility.isProSkin == false)
            {
                GUI.skin.customStyles[0].normal.textColor = defaultTextColor;
            }
            else
            {
                GUI.contentColor = defaultTextColor;
            }
        }

        public static void StyleStatus(Status status, out Color color, out Texture icon)
        {
            color = Color.clear;
            icon = null;

            float sin = Mathf.Sin((float)EditorApplication.timeSinceStartup * 3.14159274f * 2f) * 0.5f + 0.5f;

            switch (status)
            {
                case (Status)0:
                    {
                        color = Color.Lerp(new Color(97f / 255f, 255f / 255f, 66f / 255f), Color.green, sin);
                        icon = EditorGUIUtility.IconContent("vcs_check").image;
                    }
                    break;
                case (Status)1:
                    {
                        color = Color.Lerp(new Color(252f / 255f, 174f / 255f, 78f / 255f), Color.yellow, sin);
                        icon = EditorGUIUtility.IconContent("console.warnicon.sml").image;
                    }
                    break;
                case (Status)2:
                    {
                        color = Color.Lerp(new Color(255f / 255f, 112f / 255f, 112f / 255f), new Color(252f / 255f, 174f / 255f, 78f / 255f), sin);
                        icon = EditorGUIUtility.IconContent("vcs_delete").image;
                    }
                    break;
            }

            //Darken colors on personal skin
            if (EditorGUIUtility.isProSkin == false)
            {
                color = Color.Lerp(color, Color.black, 0.5f);
            }
        }

        public static void DrawLogLine(string text)
        {
            //using (new EditorGUILayout.VerticalScope())
            {
                EditorGUILayout.LabelField(new GUIContent(text, CheckMark), SCPE_GUI.LogText);
            }
        }

        public static void DrawLogLine(string text, SCPE_GUI.Status status = Status.Ok)
        {
            EditorGUILayout.LabelField(new GUIContent(text), LogText);
        }

        public static void DrawProgressBar(Rect rect, float progress)
        {
            Color defaultColor = UnityEngine.GUI.contentColor;
            Texture fillTex = Texture2D.whiteTexture;

            //Background
            GUILayout.BeginArea(rect, EditorStyles.textArea);
            {
                //Fill
                Color color = new Color(99f / 255f, 138f / 255f, 124f / 255f);
                Rect barRect = new Rect(0, 0, rect.width * progress, rect.height);
                EditorGUI.DrawRect(barRect, color);

                //EditorGUILayout.LabelField(progress * 100 + "%");
            }
            GUILayout.EndArea();

            UnityEngine.GUI.contentColor = defaultColor;

        }

        public static void DrawWindowHeader(float windowWidth, float windowHeight)
        {
            Rect headerRect = new Rect(0, 0, windowWidth, windowHeight / 6.5f);
            if (SCPE_GUI.HeaderImg)
            {
                UnityEngine.GUI.DrawTexture(headerRect, SCPE_GUI.HeaderImg, ScaleMode.ScaleToFit);
                GUILayout.Space(SCPE_GUI.HeaderImg.height / 4 + 65);
            }
            else
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("<b><size=24>SC Post Effects</size></b>\n<size=16>For Post Processing Stack</size>", Header);
            }

        }


        private static Texture2D _HeaderImg;
        public static Texture2D HeaderImg
        {
            get
            {
                if (_HeaderImg == null)
                {
                    if (HEADER_IMG_PATH == String.Empty) SCPE.GetRootFolder(); //If called before any initialization was done

                    _HeaderImg = (Texture2D)AssetDatabase.LoadAssetAtPath(HEADER_IMG_PATH, typeof(Texture2D));
                }
                return _HeaderImg;
            }
        }

        public class BoolSwitchGUI : Editor
        {
            const float height = 10f;
            const float width = 30f;
            const float tackExtrusion = 1f;

            static Color onColor = new Color(99f / 255f, 138f / 255f, 124f / 255f); //Staggart
            static Color fillColor = onColor * 0.66f;
            static float offBrightness = 0.33f;
            static Color offColor = new Color(offBrightness, offBrightness, offBrightness, 1f);

            static GUIStyle _textStyle;
            static GUIStyle textStyle
            {
                get
                {
                    if (_textStyle == null)
                    {
                        _textStyle = new GUIStyle(EditorStyles.miniLabel);
                        _textStyle.fontSize = 9;
                        _textStyle.alignment = TextAnchor.MiddleLeft;
                    }
                    return _textStyle;
                }
            }

            static GUIStyle _backGroundStyle;
            static GUIStyle backGroundStyle
            {
                get
                {
                    if (_backGroundStyle == null)
                    {
                        _backGroundStyle = new GUIStyle(EditorStyles.miniTextField);
                        _backGroundStyle.fixedWidth = width;
                        _backGroundStyle.fixedHeight = height;
                    }
                    return _backGroundStyle;
                }
            }

            private static bool Draw(bool value)
            {

                Rect rect = GUILayoutUtility.GetLastRect();
                float prefix = EditorGUIUtility.labelWidth;
                rect.x += prefix * 2f;
                rect.y += 2f;

                //Background functions as a button
                value = (GUI.Toggle(rect, value, "", backGroundStyle));

                //Fill with color when enabled
                Rect fillrect = new Rect(rect.x + 1, rect.y + 1, (value ? width : 0), height - 2);
                if (value == true) EditorGUI.DrawRect(fillrect, fillColor);

                //Shift tack from left to right
                Rect tackRect = new Rect(rect.x + (value ? width / 2f + 1f : 0f), rect.y - tackExtrusion, width / 2f, height + tackExtrusion + 1f);
                EditorGUI.DrawRect(tackRect, (value ? onColor : offColor));

                textStyle.padding = new RectOffset((value ? 19 : 4), 0, -2, 0);
                //GUI.Label(new Rect(rect.x, rect.y, width, height), value ? "ON" : "OFF", textStyle);
                //GUI.Label(new Rect(rect.x, rect.y, width, height), "≡", textStyle);

                return value;
            }

            public static bool Draw(bool value, string text)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(text);

                    value = BoolSwitchGUI.Draw(value);
                }

                return value;
            }

            public static bool Draw(bool value, GUIContent content)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(content);

                    value = BoolSwitchGUI.Draw(value);
                }

                return value;
            }


        }


        #region Styles
        private static Texture _HelpIcon;
        public static Texture HelpIcon
        {
            get
            {
                if (_HelpIcon == null)
                {
                    _HelpIcon = EditorGUIUtility.FindTexture("_Help");
                }
                return _HelpIcon;
            }
        }

        private static Texture _InfoIcon;
        public static Texture InfoIcon
        {
            get
            {
                if (_InfoIcon == null)
                {
                    _InfoIcon = EditorGUIUtility.FindTexture("d_UnityEditor.InspectorWindow");
                }
                return _InfoIcon;
            }
        }

        private static GUIStyle _Footer;
        public static GUIStyle Footer
        {
            get
            {
                if (_Footer == null)
                {
                    _Footer = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
                    {
                        alignment = TextAnchor.LowerCenter,
                        wordWrap = true,
                        fontSize = 12
                    };
                }

                return _Footer;
            }
        }

        private static Texture _CheckMark;
        public static Texture CheckMark
        {
            get
            {
                if (_CheckMark == null)
                {
                    _CheckMark = EditorGUIUtility.IconContent("vcs_check").image;
                }
                return _CheckMark;
            }
        }

        private static GUIStyle _StatusBox;
        public static GUIStyle StatusBox
        {
            get
            {
                if (_StatusBox == null)
                {
                    _StatusBox = new GUIStyle(EditorStyles.textArea)
                    {
                        alignment = TextAnchor.MiddleLeft,
                        fixedWidth = 200f
                    };
                }

                return _StatusBox;
            }
        }


        private static GUIStyle _LogText;
        public static GUIStyle LogText
        {
            get
            {
                if (_LogText == null)
                {
                    _LogText = new GUIStyle(EditorStyles.label)
                    {
                        alignment = TextAnchor.UpperLeft,
                        richText = true,
                        wordWrap = true,
                        stretchHeight = false,
                        stretchWidth = false,
                        fontStyle = FontStyle.Normal,
                        fontSize = 11
                    };
                }

                return _LogText;
            }
        }

        private static GUIStyle _PathField;
        public static GUIStyle PathField
        {
            get
            {
                if (_PathField == null)
                {
                    _PathField = new GUIStyle(EditorStyles.textField)
                    {
                        alignment = TextAnchor.MiddleRight
                    };
                }

                return _PathField;
            }
        }

        #region Toggles
        private static GUIStyle _ToggleButtonLeftNormal;
        public static GUIStyle ToggleButtonLeftNormal
        {
            get
            {
                if (_ToggleButtonLeftNormal == null)
                {
                    _ToggleButtonLeftNormal = new GUIStyle(EditorStyles.miniButtonLeft)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        stretchWidth = true,
                        richText = true,
                        wordWrap = false,
                        fixedHeight = 20f,
                        fixedWidth = 105f
                    };
                }

                return _ToggleButtonLeftNormal;
            }
        }
        private static GUIStyle _ToggleButtonLeftToggled;
        public static GUIStyle ToggleButtonLeftToggled
        {
            get
            {
                if (_ToggleButtonLeftToggled == null)
                {
                    _ToggleButtonLeftToggled = new GUIStyle(ToggleButtonLeftNormal);
                    _ToggleButtonLeftToggled.normal.background = _ToggleButtonLeftToggled.active.background;
                }

                return _ToggleButtonLeftToggled;
            }
        }

        private static GUIStyle _ToggleButtonRightNormal;
        public static GUIStyle ToggleButtonRightNormal
        {
            get
            {
                if (_ToggleButtonRightNormal == null)
                {
                    _ToggleButtonRightNormal = new GUIStyle(EditorStyles.miniButtonRight)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        stretchWidth = true,
                        richText = true,
                        wordWrap = false,
                        fixedHeight = 20f,
                        fixedWidth = 105f

                    };
                }

                return _ToggleButtonRightNormal;
            }
        }

        private static GUIStyle _ToggleButtonRightToggled;
        public static GUIStyle ToggleButtonRightToggled
        {
            get
            {
                if (_ToggleButtonRightToggled == null)
                {
                    _ToggleButtonRightToggled = new GUIStyle(ToggleButtonRightNormal);
                    _ToggleButtonRightToggled.normal.background = _ToggleButtonRightToggled.active.background;
                }

                return _ToggleButtonRightToggled;
            }
        }
        #endregion

        #region Buttons
        private static GUIStyle _Button;
        public static GUIStyle Button
        {
            get
            {
                if (_Button == null)
                {
                    _Button = new GUIStyle(UnityEngine.GUI.skin.button)
                    {
                        alignment = TextAnchor.MiddleLeft,
                        stretchWidth = true,
                        richText = true,
                        wordWrap = true,
                        padding = new RectOffset()
                        {
                            left = 14,
                            right = 14,
                            top = 8,
                            bottom = 8
                        }
                    };
                }

                return _Button;
            }
        }

        private static GUIStyle _DocButton;
        public static GUIStyle DocButton
        {
            get
            {
                if (_DocButton == null)
                {
                    _DocButton = new GUIStyle(EditorStyles.miniButton)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        stretchWidth = false,
                        richText = true,
                        wordWrap = true,
                        fixedHeight = 17f,
                        fixedWidth = 25f,
                        margin = new RectOffset()
                        {
                            left = 0,
                            right = 75,
                            top = 0,
                            bottom = 0
                        }
                        
                    };
                }

                return _DocButton;
            }
        }

        private static GUIStyle _ProgressButtonLeft;
        public static GUIStyle ProgressButtonLeft
        {
            get
            {
                if (_ProgressButtonLeft == null)
                {
                    _ProgressButtonLeft = new GUIStyle(EditorStyles.miniButtonLeft)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fixedHeight = 30f,
                        stretchWidth = true,
                        stretchHeight = true,
                        richText = true,
                        wordWrap = true,
                        fontSize = 12,
                        fontStyle = FontStyle.Normal
                    };
                }

                return _ProgressButtonLeft;
            }
        }

        private static GUIStyle _ProgressButtonRight;
        public static GUIStyle ProgressButtonRight
        {
            get
            {
                if (_ProgressButtonRight == null)
                {
                    _ProgressButtonRight = new GUIStyle(EditorStyles.miniButtonRight)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fixedHeight = 30f,
                        stretchWidth = true,
                        stretchHeight = true,
                        richText = true,
                        wordWrap = true,
                        fontSize = 12,
                        fontStyle = FontStyle.Bold
                    };
                }

                return _ProgressButtonRight;
            }
        }
        #endregion

        private static GUIStyle _ProgressTab;
        public static GUIStyle ProgressTab
        {
            get
            {
                if (_ProgressTab == null)
                {
                    _ProgressTab = new GUIStyle(EditorStyles.toolbarButton)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fixedHeight = 25f,
                        stretchWidth = true,
                        stretchHeight = true,
                        richText = true,
                        wordWrap = true,
                        fontSize = 12,
                        fontStyle = FontStyle.Bold
                    };
                }

                return _ProgressTab;
            }
        }

        private static GUIStyle _Tab;
        public static GUIStyle Tab
        {
            get
            {
                if (_Tab == null)
                {
                    _Tab = new GUIStyle(EditorStyles.miniButtonMid)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        stretchWidth = true,
                        richText = true,
                        wordWrap = true,
                        fontSize = 12,
                        fontStyle = FontStyle.Bold,
                        padding = new RectOffset()
                        {
                            left = 14,
                            right = 14,
                            top = 8,
                            bottom = 8
                        }
                    };
                }

                return _Tab;
            }
        }

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
                        alignment = TextAnchor.MiddleLeft,
                        wordWrap = true,
                        fontSize = 18,
                        fontStyle = FontStyle.Bold,
                        padding = new RectOffset()
                        {
                            left = 5,
                            right = 0,
                            top = 0,
                            bottom = 0
                        }
                    };
                }

                return _Header;
            }
        }
        #endregion
    }
}