using UnityEngine;
using TMPro;
using System.Collections;

namespace UI.LoadingScreen
{
    public class LoadingScreen : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI loadingText;
        
        [SerializeField] 
        private CanvasGroup canvasGroup;
        
        [SerializeField] 
        private float fadeDuration = 0.5f;

        public void Show(string message = "Loading...")
        {
            if (loadingText)
                loadingText.text = message;

            gameObject.SetActive(true);
        }
        
        public void Hide()
        {
            StartCoroutine(FadeOut());
        }

        private IEnumerator FadeOut()
        {
            float time = 0f;
            while (time < fadeDuration)
            {
                time += Time.deltaTime;
                canvasGroup.alpha = 1f - (time / fadeDuration);
                yield return null;
            }

            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            gameObject.SetActive(false);
        }
    }
}