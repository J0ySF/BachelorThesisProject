using UnityEngine;

namespace Loading.Menu
{
    /// <summary>
    /// Simple spinning icon controller, listens to loading events to show/hide the icon.
    /// </summary>
    public sealed class Indicator : Listener
    {
        [SerializeField] private GameObject icon;
        [SerializeField] private float rotationSpeed = 180.0f;

        private void Start() => OnFinishLoading();

        private void Update() => transform.Rotate(0, 0, -rotationSpeed * Time.deltaTime);
        
        public override void OnStartLoading() => icon.SetActive(true);
        public override void OnFinishLoading() => icon.SetActive(false);
    }
}