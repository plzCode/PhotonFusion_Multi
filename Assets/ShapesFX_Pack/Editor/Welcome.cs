using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;

namespace Orangedkeys.ShapesFX
{
    public class Welcome : EditorWindow
    {
        // Google Form constants for subscribing
        private const string kGFormBaseURL = "https://docs.google.com/forms/d/e/1FAIpQLSfC-ISb57A-WfUnZPXx0eSXBHaWPAML4StnOwSGelPuyosMxA/";
        private const string kGFormEntryID = "entry.837402547";
        private const string kGFormEntryID2 = "entry.1451810422";
        private const string kGFormEntryID3 = "entry.1123184567";

        // URLs
        public const string WEB_URL = "https://www.orangedkeys.com";
        public const string WEB_URL_Files = "https://orangedkeys.gumroad.com/l/shapesfxgenerator";
        public const string YOUTUBE_URL = "https://www.youtube.com/channel/UC68I9tTol5hAoyVVAVedPag";

        // Variables for user input
        private string email;
        private string ownername;

        // Window dimensions
        private static readonly int WelcomeWindowWidth = 512;
        private static readonly int WelcomeWindowHeight = 682;

        // Styles
        private static GUIStyle _largeTextStyle;
        public static GUIStyle LargeTextStyle
        {
            get
            {
                if (_largeTextStyle == null)
                {
                    _largeTextStyle = new GUIStyle(GUI.skin.label)
                    {
                        richText = true,
                        wordWrap = true,
                        fontStyle = FontStyle.Bold,
                        fontSize = 14,
                        alignment = TextAnchor.MiddleLeft,
                        padding = new RectOffset(0, 0, 0, 0)
                    };
                }
                _largeTextStyle.normal.textColor = new Color32(200, 100, 0, 255);
                return _largeTextStyle;
            }
        }

        private static GUIStyle _regularTextStyle;
        public static GUIStyle RegularTextStyle
        {
            get
            {
                if (_regularTextStyle == null)
                {
                    _regularTextStyle = new GUIStyle(GUI.skin.label)
                    {
                        richText = true,
                        wordWrap = true,
                        fontStyle = FontStyle.Normal,
                        fontSize = 12,
                        alignment = TextAnchor.MiddleLeft,
                        padding = new RectOffset(0, 0, 0, 0)
                    };
                }
                return _regularTextStyle;
            }
        }

        private static GUIStyle _footerTextStyle;
        public static GUIStyle FooterTextStyle
        {
            get
            {
                if (_footerTextStyle == null)
                {
                    _footerTextStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
                    {
                        alignment = TextAnchor.LowerCenter,
                        wordWrap = true,
                        fontSize = 12
                    };
                }
                return _footerTextStyle;
            }
        }

        // Menu item to show the window
        [MenuItem("Tools/OrangedKeys/ShapesFX_Pack/About", false, 0)]
        public static void ShowWindow()
        {
            EditorWindow editorWindow = GetWindow(typeof(Welcome), false, "About", true);
            editorWindow.autoRepaintOnSceneChange = true;
            editorWindow.titleContent.image = EditorGUIUtility.IconContent("animationdopesheetkeyframe").image;
            editorWindow.maxSize = new Vector2(WelcomeWindowWidth, WelcomeWindowHeight);
            editorWindow.minSize = new Vector2(WelcomeWindowWidth, WelcomeWindowHeight);
            editorWindow.position = new Rect(Screen.width / 2 + WelcomeWindowWidth / 2, Screen.height / 2, WelcomeWindowWidth, WelcomeWindowHeight);
            editorWindow.Show();
        }

        private void OnGUI()
        {
            // Initialize the custom button style
            GUIStyle customButtonStyle = new GUIStyle(GUI.skin.button)
            {
                normal = { textColor = Color.white },
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            // Create a background texture with the desired color (orange)
            Texture2D backgroundTexture = new Texture2D(1, 1);
            Color orangeColor = new Color32(200, 100, 0, 255);
            backgroundTexture.SetPixel(0, 0, orangeColor);
            backgroundTexture.Apply();

            customButtonStyle.normal.background = backgroundTexture;
            customButtonStyle.hover.background = backgroundTexture;
            customButtonStyle.active.background = backgroundTexture;

            // Set button size
            float buttonWidth = 480f;
            float buttonHeight = 60f;

            if (EditorApplication.isCompiling)
            {
                ShowNotification(new GUIContent("Compiling Scripts", EditorGUIUtility.IconContent("BuildSettings.Editor").image));
            }
            else
            {
                RemoveNotification();
            }

            // Add The Banner
            Texture2D welcomeImage = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/ShapesFX_Pack/Resources/ShapesFX_PACK.png", typeof(Texture2D));
            Rect welcomeImageRect = new Rect(0, 0, 512, 128);
            GUI.DrawTexture(welcomeImageRect, welcomeImage);

            //GUILayout.Space(20);

            GUILayout.BeginArea(new Rect(EditorGUILayout.GetControlRect().x + 10, 200, WelcomeWindowWidth - 20, WelcomeWindowHeight));

            EditorGUILayout.LabelField("Have fun with ''SHAPES FX PACK'' !! \n", LargeTextStyle);
            EditorGUILayout.LabelField("Join my Circle for exclusive updates and Cool STUFF!! \n", RegularTextStyle);
            EditorGUILayout.Space();

            // Subscribe section
            ownername = EditorGUILayout.TextField("Name: ", ownername, GUILayout.MaxWidth(480f));
            email = EditorGUILayout.TextField("E-mail: ", email, GUILayout.MaxWidth(480f));
            EditorGUILayout.Space();

            if (GUILayout.Button(new GUIContent("Join!", EditorGUIUtility.IconContent("d_orangeLight").image), GUILayout.MaxWidth(480)))
            {
                SendGFormData(email);
            }

            // Website button
            if (GUILayout.Button(new GUIContent("www.OrangedKeys.com", EditorGUIUtility.IconContent("BuildSettings.Web.Small").image), GUILayout.MaxWidth(480)))
            {
                Application.OpenURL(WEB_URL);
            }

            // YouTube button
            if (GUILayout.Button(new GUIContent("YouTube Channel", EditorGUIUtility.IconContent("Animation.Record").image), GUILayout.MaxWidth(480)))
            {
                Application.OpenURL(YOUTUBE_URL);
            }

            EditorGUILayout.Space(45);

            // Update section
            EditorGUILayout.LabelField("Hot News  : ---------------------------------------------------->>>", LargeTextStyle);
            EditorGUILayout.LabelField("Source files now available for your own creations..unlimited Possibilities", RegularTextStyle);
            if (GUILayout.Button(new GUIContent("Check Out My Gumroad", EditorGUIUtility.IconContent("BuildSettings.Web.Small").image), customButtonStyle, GUILayout.Width(buttonWidth), GUILayout.Height(buttonHeight)))
            {
                Application.OpenURL(WEB_URL_Files);
            }
            EditorGUILayout.LabelField("<<<------------------------------------------------------------>>>", LargeTextStyle);
          
            EditorGUILayout.Space(20);

            GUILayout.EndArea();

            Rect areaRect = new Rect(0, WelcomeWindowHeight - 20, WelcomeWindowWidth, WelcomeWindowHeight - 20);
            GUILayout.BeginArea(areaRect);
            EditorGUILayout.LabelField("Copyright © 2024 OrangedKeys", FooterTextStyle);
            EditorGUILayout.Space();
            GUILayout.EndArea();
        }

        private void OnInspectorUpdate()
        {
            Repaint();
        }

        // Send data to Google Forms
        public void SendGFormData(string userEmail)
        {
            string assetName = "ShapesFX,";
            WWWForm form = new WWWForm();
            form.AddField(kGFormEntryID, userEmail);
            form.AddField(kGFormEntryID2, assetName);
            form.AddField(kGFormEntryID3, ownername);

            string urlGFormResponse = kGFormBaseURL + "formResponse";
            UnityWebRequest www = UnityWebRequest.Post(urlGFormResponse, form);
            www.SendWebRequest();

            if (www == null)
            {
                Debug.Log("Subscribe error: can't build request");
            }
            else
            {
                // Handle the response
                if (string.IsNullOrEmpty(www.error))
                {
                    Debug.Log("Subscribe success");
                }
                else
                {
                    Debug.Log("Subscribe error: " + www.error);
                }
            }
        }
    }
}