using System.Collections.Generic;
using UnityEngine;

namespace Intelmatix
{
    /// <summary>
    /// Manager of a custom Screensaver that shows a video, a cube animation and a list of project icons.
    /// </summary>
    public class Screensaver : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CubeBehaviorController cubeController;
        [SerializeField] private CanvasGroup screensaverCanvasGroup;
        [SerializeField] private CanvasGroup logosCanvasGroup;
        [SerializeField] private CanvasGroup videoCanvasGroup;
        [SerializeField] private CanvasGroup cubesCanvasGroup;
        [SerializeField] private ProjectButton projectButtonTemplate;
        [SerializeField] private Canvas logosCanvas;
        [SerializeField] private ProjectDisplayInfo[] projects;

        [Header("Settings")]
        [SerializeField, Min(0)] private float inactivityDuration = 240f;

        [Header("Initialization")]
        [SerializeField] private bool startOnAwake = true;
        public static Screensaver Instance;

        private bool isScreensaverActive = true;
        private float inactivityTimer;
        private static bool projectOpened;
        private bool wasTouchingLastFrame;
        private readonly List<RectTransform> displayedProjects = new();

        private void Awake()
        {
            Instance = this;
            ResetInactivityTimer();

            if (startOnAwake)
                ActivateScreensaver();

            foreach (ProjectDisplayInfo project in projects)
            {
                var instance = Instantiate(projectButtonTemplate, logosCanvasGroup.transform);
                instance.SetData(project);
                displayedProjects.Add(instance.Icon.GetComponent<RectTransform>());
            }
        }

        private void Update()
        {
            bool touch = Input.touchCount > 0 && !wasTouchingLastFrame;
            wasTouchingLastFrame = Input.touchCount > 0;
            bool inputReceived = Input.anyKeyDown || Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2) || touch;

            if (inputReceived)
            {
                ResetInactivityTimer();
            }

            inputReceived = inputReceived && screensaverCanvasGroup.alpha > 0.7f;

            if (isScreensaverActive && inputReceived)
            {
                if (projectOpened)
                    DeactivateScreensaver();
                else
                    ShowButtons();
            }
            else if (!isScreensaverActive)
            {
                inactivityTimer += Time.deltaTime;
                if (inactivityTimer >= inactivityDuration)
                    ActivateScreensaver();
            }

            screensaverCanvasGroup.blocksRaycasts = screensaverCanvasGroup.alpha > 0.1f;
        }

        private void ShowButtons()
        {
            cubeController.Stop();

            cubeController.MoveCubesToPositions(displayedProjects.ToArray(), logosCanvas.scaleFactor);

            logosCanvasGroup.LeanAlpha(1, 0.3f).setDelay(2.5f).setOnComplete(() =>
            {
                logosCanvasGroup.blocksRaycasts = true;

                cubesCanvasGroup.LeanAlpha(0, 0.3f);
                cubeController.Deactivate();
            });
        }

        public void ActivateScreensaver(bool fullreset = false)
        {
            ResetScreensaver(fullreset);
            screensaverCanvasGroup.LeanAlpha(1, 0.5f);
        }

        public void DeactivateScreensaver()
        {
            projectOpened = true;
            isScreensaverActive = false;
            screensaverCanvasGroup.LeanAlpha(0, 0.5f);
            cubeController.Deactivate();
        }

        private void ResetScreensaver(bool fullreset = false)
        {
            cubesCanvasGroup.alpha = 1;
            videoCanvasGroup.alpha = 1;
            cubeController.Collapse();
            logosCanvasGroup.alpha = 0;
            logosCanvasGroup.blocksRaycasts = false;
            isScreensaverActive = true;
            if (fullreset) projectOpened = false;
            ResetInactivityTimer();

            LeanTween.cancel(logosCanvasGroup.gameObject);
        }

        private void ResetInactivityTimer() => inactivityTimer = 0f;
    }
}
