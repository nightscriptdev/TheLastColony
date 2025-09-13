using System.Collections;
using Core;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Managers
{
    public class LightingManager : MonoBehaviour
    {
        [Header("光照设置")] [Tooltip("场景中的全局光，用来模拟太阳/月亮的光照")] [SerializeField]
        private Light2D globalLight;

        [Header("白天参数")] [Tooltip("白天时的全局光颜色")] [SerializeField]
        private Color dayColor = Color.white;

        [Tooltip("白天时的全局光强度")] [SerializeField] [Range(0f, 2f)]
        private float dayIntensity = 1f;

        [Header("夜晚参数")] [Tooltip("夜晚时的全局光颜色")] [SerializeField]
        private Color nightColor = new Color(0.1f, 0.1f, 0.3f); // 一个偏冷的暗蓝色

        [Tooltip("夜晚时的全局光强度")] [SerializeField] [Range(0f, 2f)]
        private float nightIntensity = 0.4f;

        [Header("过渡参数")] [Tooltip("从一个状态（白天/夜晚）过渡到另一个状态需要的时间（秒）")] [SerializeField]
        private float transitionDuration = 3.0f;

        // 存储当前正在运行的过渡协程，以便在需要时可以打断它
        private Coroutine _currentTransitionCoroutine;

        // --- Unity生命周期函数 ---


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
            // 游戏开始时，为了保险起见，直接设置为白天的状态
            // 这样可以避免在编辑器里是夜晚设置，导致游戏一运行就是晚上
            SetToDayImmediate();
        }

        // --- 公共方法 ---

        /// <summary>
        /// 立即将光照设置为白天状态，无过渡效果。
        /// </summary>
        public void SetToDayImmediate()
        {
            if (globalLight == null) return;
            globalLight.color = dayColor;
            globalLight.intensity = dayIntensity;
        }

        /// <summary>
        /// 立即将光照设置为夜晚状态，无过渡效果。
        /// </summary>
        public void SetToNightImmediate()
        {
            if (globalLight == null) return;
            globalLight.color = nightColor;
            globalLight.intensity = nightIntensity;
        }


        /// <summary>
        /// 启动向白天的过渡。
        /// </summary>
        public void StartTransitionToDay(int day)
        {
            // 在开始新的过渡之前，先停止任何正在进行的过渡，防止冲突
            if (_currentTransitionCoroutine != null)
            {
                StopCoroutine(_currentTransitionCoroutine);
            }

            _currentTransitionCoroutine = StartCoroutine(TransitionTo(dayColor, dayIntensity));
        }

        /// <summary>
        /// 启动向夜晚的过渡。
        /// </summary>
        public void StartTransitionToNight(int day)
        {
            // 在开始新的过渡之前，先停止任何正在进行的过渡，防止冲突
            if (_currentTransitionCoroutine != null)
            {
                StopCoroutine(_currentTransitionCoroutine);
            }

            _currentTransitionCoroutine = StartCoroutine(TransitionTo(nightColor, nightIntensity));
        }

        // --- 核心逻辑：协程 ---

        /// <summary>
        /// 一个协程，负责在指定的时间内，将全局光平滑地从当前状态过渡到目标状态。
        /// </summary>
        /// <param name="targetColor">目标颜色</param>
        /// <param name="targetIntensity">目标强度</param>
        /// <returns>IEnumerator for the coroutine</returns>
        private IEnumerator TransitionTo(Color targetColor, float targetIntensity)
        {
            if (globalLight == null)
            {
                Debug.LogError("全局光(Global Light 2D)未被指定！");
                yield break; // 提前退出协程
            }

            // 记录过渡开始时的初始状态
            Color startColor = globalLight.color;
            float startIntensity = globalLight.intensity;

            // 记录过渡已经花费的时间
            float elapsedTime = 0f;

            // 当过渡时间还未结束时，循环执行
            while (elapsedTime < transitionDuration)
            {
                // 计算当前过渡的进度（0到1之间）
                // Time.deltaTime 是上一帧到当前帧的时间，保证过渡是平滑且与帧率无关的
                float progress = elapsedTime / transitionDuration;

                // 使用线性插值 (Lerp) 来计算当前帧应该设置的颜色和强度
                // Lerp函数根据进度(t)在起始值和结束值之间进行插值
                globalLight.color = Color.Lerp(startColor, targetColor, progress);
                globalLight.intensity = Mathf.Lerp(startIntensity, targetIntensity, progress);

                // 更新已用时间
                elapsedTime += Time.deltaTime;

                // 等待下一帧，然后继续循环
                yield return null;
            }

            // 循环结束后，为确保最终状态精确无误，直接设置为目标值
            globalLight.color = targetColor;
            globalLight.intensity = targetIntensity;

            // 标记协程已执行完毕
            _currentTransitionCoroutine = null;
        }
    }
}