using UnityEngine;
using TMPro;
using System.Collections;

namespace UI.LoadingScreen
{
    public class LoadingScreen : MonoBehaviour
    {
        [SerializeField] 
        private CanvasGroup canvasGroup;
        
        [SerializeField] 
        private float fadeDuration = 0.5f;

        public void Show()
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            StartCoroutine(FadeOut());
        }

        private IEnumerator FadeOut()
        {
            float t = 0f;

            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                canvasGroup.alpha = 1f - (t / fadeDuration);
                yield return null;
            }

            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            gameObject.SetActive(false);
        }
    }
}