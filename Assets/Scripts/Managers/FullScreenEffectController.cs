using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Managers
{
    public class FullScreenEffectController : MonoBehaviour
    {
        public static FullScreenEffectController Instance { get; private set; }
        
        [Header("Time Stats")]
        [SerializeField] private float timeToFadeIn = 1.5f;
        [SerializeField] private float timeToFadeOut = 0.5f;
        
        [Header("References")] 
        [SerializeField] private ScriptableRendererFeature fullScreenDamage;
        [SerializeField] private Material fullScreenDamageMaterial;
        
        [Header("Intensity Stats")]
        [SerializeField] private float voronoiIntensityStartAmount = 2.5f;
        [SerializeField] private float vignetteIntensityStartAmount = 1.25f;
        
        private readonly int _voronoiIntensity = Shader.PropertyToID("_VoronoiIntensity");
        private readonly int _vignetteIntensity = Shader.PropertyToID("_VignetteIntensity");

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            fullScreenDamage.SetActive(false);
        }

        public void TriggerEffect()
        {
            if (!gameObject.activeInHierarchy)
            {
                gameObject.SetActive(true);
            }
            StartCoroutine(Hurt());
        }

        private IEnumerator Hurt()
        {
            fullScreenDamage.SetActive(true);
            fullScreenDamageMaterial.SetFloat(_voronoiIntensity, voronoiIntensityStartAmount);
            fullScreenDamageMaterial.SetFloat(_vignetteIntensity, vignetteIntensityStartAmount);
            
            yield return new WaitForSeconds(timeToFadeIn);
            
            float elapsedTime = 0f;
            while (elapsedTime < timeToFadeOut)
            {
                elapsedTime += Time.deltaTime; // or Runner.DeltaTime if needed
                
                float lerpedVoronoi = Mathf.Lerp(voronoiIntensityStartAmount, 0f, elapsedTime / timeToFadeOut);
                float lerpedVignette = Mathf.Lerp(vignetteIntensityStartAmount, 0f, elapsedTime / timeToFadeOut);
                
                fullScreenDamageMaterial.SetFloat(_voronoiIntensity, lerpedVoronoi);
                fullScreenDamageMaterial.SetFloat(_vignetteIntensity, lerpedVignette);
                
                yield return null;
            }
            fullScreenDamage.SetActive(false);
        }
    }
}
