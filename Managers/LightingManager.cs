using System.Collections;
using Core;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Managers
{
    public class LightingManager : MonoBehaviour
    {
        [SerializeField]
        private Light2D globalLight;

        [SerializeField]
        private Color dayColor = Color.white;

        [SerializeField] [Range(0f, 2f)]
        private float dayIntensity = 1f;

        [SerializeField]
        private Color nightColor = new Color(0.1f, 0.1f, 0.3f);

        [SerializeField] [Range(0f, 2f)]
        private float nightIntensity = 0.4f;

        [Header("过渡参数")][SerializeField]
        private float transitionDuration = 3.0f;

        private Coroutine _currentTransitionCoroutine;

        private void OnEnable()
        {
            EventManager.OnDayStart += StartTransitionToDay;
            EventManager.OnNightStart += StartTransitionToNight;
        }

        private void OnDisable()
        {
            EventManager.OnDayStart -= StartTransitionToDay;
            EventManager.OnNightStart -= StartTransitionToNight;
        }

        private void Start()
        {
            SetToDayImmediate();
        }

        public void SetToDayImmediate()
        {
            if (globalLight == null) return;
            globalLight.color = dayColor;
            globalLight.intensity = dayIntensity;
        }

        public void SetToNightImmediate()
        {
            if (globalLight == null) return;
            globalLight.color = nightColor;
            globalLight.intensity = nightIntensity;
        }

        public void StartTransitionToDay(int day)
        {
            if (_currentTransitionCoroutine != null)
            {
                StopCoroutine(_currentTransitionCoroutine);
            }

            _currentTransitionCoroutine = StartCoroutine(TransitionTo(dayColor, dayIntensity));
        }

        public void StartTransitionToNight(int day)
        {
            if (_currentTransitionCoroutine != null)
            {
                StopCoroutine(_currentTransitionCoroutine);
            }

            _currentTransitionCoroutine = StartCoroutine(TransitionTo(nightColor, nightIntensity));
        }

        private IEnumerator TransitionTo(Color targetColor, float targetIntensity)
        {
            if (globalLight == null)
            {
                yield break;
            }

            Color startColor = globalLight.color;
            float startIntensity = globalLight.intensity;

            float elapsedTime = 0f;

            while (elapsedTime < transitionDuration)
            {
                float progress = elapsedTime / transitionDuration;

                globalLight.color = Color.Lerp(startColor, targetColor, progress);
                globalLight.intensity = Mathf.Lerp(startIntensity, targetIntensity, progress);

                elapsedTime += Time.deltaTime;

                yield return null;
            }

            globalLight.color = targetColor;
            globalLight.intensity = targetIntensity;

            _currentTransitionCoroutine = null;
        }
    }
}