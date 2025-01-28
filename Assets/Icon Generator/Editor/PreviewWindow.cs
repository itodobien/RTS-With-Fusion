using UnityEditor;
using UnityEngine;

namespace LatinTools.Converter
{
    public class PreviewWindow : EditorWindow
    {
        private Texture2D previewTexture;
        private IconGeneratorWindow parentWindow;

        public static PreviewWindow ShowWindow(Texture2D preview, IconGeneratorWindow parent)
        {
            PreviewWindow window = GetWindow<PreviewWindow>("Preview", true);
            window.previewTexture = preview;
            window.parentWindow = parent;
            window.Show();
            return window;
        }

        private void OnGUI()
        {
            if (previewTexture != null)
            {
                EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), new Color ( 0f, 0f, 0f));

                float aspectRatio = (float)previewTexture.width / previewTexture.height;
                float targetWidth = position.width;
                float targetHeight = targetWidth / aspectRatio;

                if (targetHeight > position.height)
                {
                    targetHeight = position.height;
                    targetWidth = targetHeight * aspectRatio;
                }

                float xOffset = (position.width - targetWidth) * 0.5f;
                float yOffset = (position.height - targetHeight) * 0.5f;
                Rect previewRect = new Rect(xOffset, yOffset, targetWidth, targetHeight);

                if (parentWindow.icon_settings.isTransparent && !parentWindow.HideAlphaInTexturePreview)
                {
                    EditorGUI.DrawRect(previewRect, Color.black);
                    DrawCheckerBackground(previewRect);
                }

                GUI.DrawTexture(previewRect, previewTexture, ScaleMode.ScaleToFit);
                parentWindow.HandleDragEvents(previewRect);
            }
        }

        private void Update()
        {
            if (parentWindow != null)
            {
                previewTexture = parentWindow.Preview;
                Repaint();
            } 
            if (!parentWindow.showPreviewInMainEditor)
            {
                Close();
            }
        }

        private void OnDestroy()
        {
            if (parentWindow != null)
            {
                parentWindow.showPreviewInMainEditor = false;
            }
        }

        private void DrawCheckerBackground(Rect rect)
        {
            int checkerSize = 20;
            Color color1 = new Color(0.8f, 0.8f, 0.8f);
            Color color2 = new Color(0.9f, 0.9f, 0.9f);

            GUI.BeginGroup(rect);

            int numCols = Mathf.CeilToInt(rect.width / checkerSize);
            int numRows = Mathf.CeilToInt(rect.height / checkerSize);

            for (int y = 0; y < numRows; y++)
            {
                for (int x = 0; x < numCols; x++)
                {
                    Rect checkerRect = new Rect(
                        x * checkerSize,
                        y * checkerSize,
                        checkerSize,
                        checkerSize
                    );

                    bool isEven = (x + y) % 2 == 0;
                    EditorGUI.DrawRect(checkerRect, isEven ? color1 : color2);
                }
            }

            GUI.EndGroup();
        }
    }
}