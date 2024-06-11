using UI.Sheet;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils;
using Graphics = Score.Graphics;

namespace UI
{
    // TODO: separate responsabilities.
    // TODO: improve code quality.

    /// <summary>
    /// Sheet view implementation that uses both an Unity scroll rect and a scroll bar to scroll.
    /// <para>All viewport parameters are initialized during start, so editing the scene's components transforms may
    /// break the functionality.</para>
    /// </summary>
    public sealed class SheetScroller : MonoBehaviour, IView
    {
        public IController Controller { get; set; } // TODO: support changing controller

        private bool _interactable = true;

        public bool Interactable
        {
            get => _interactable;
            set
            {
                // Handle the scroll view and scrollbar interactivity to avoid recieving events.
                _interactable = value;
                if (_interactable)
                {
                    scrollRect.horizontal = true;
                    scrollBar.interactable = true;
                    scrollRect.viewport.GetComponent<Image>().raycastTarget = true; // The only way to disable some events.
                    _cursorImage.color = enabledCursorColor;
                }
                else
                {
                    scrollRect.horizontal = false;
                    scrollBar.interactable = false;
                    scrollRect.viewport.GetComponent<Image>().raycastTarget = false; // The only way to disable some events.
                    _cursorImage.color = disabledCursorColor;
                }
            }
        }

        [SerializeField] private Image segmentPrefab;
        [SerializeField] private HorizontalLayoutGroup segmentsParent;

        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private Scrollbar scrollBar;

        [SerializeField] private RectTransform cursor;

        [SerializeField] private Color enabledCursorColor;
        [SerializeField] private Color disabledCursorColor;

        /// <summary>The cached beat cursor's image component.</summary>
        private Image _cursorImage;

        /// <summary>The sheet segment pool to avoid unnecessary instantiation while loading.</summary>
        private OrderedPool<Image> _sheetSegmentsPool;

        /// <summary>The cached beat cursor's horizontal position.</summary>
        /// <para>The scroll position should always be centered on the cursor position.</para>
        private int _cursorPosition;

        /// <summary>The cached segmentsParent transform, which is used to move the entire sheet horizontally.</summary>
        private RectTransform _segmentsParentTransform;

        /// <summary>The cached viewport width.</summary>
        private int _viewportWidth;

        /// <summary>The cached viewport width.</summary>
        private int _viewportHeight;

        /// <summary>The sheet's "whitespace" width first beat.</summary>
        private int _sheetOffsetStart;

        /// <summary>The current score's first beat's position.</summary>
        private int _firstValidPosition;

        /// <summary>The scroll position taking into account for the cursor's position and sheet offset.</summary>
        private int ScrollPosition
        {
            get
            {
                var position = -_segmentsParentTransform.anchoredPosition.x + _cursorPosition - _sheetOffsetStart;
                if (position < _firstValidPosition) position = _firstValidPosition;
                return (int)position;
            }
        }

        /// <summary>
        /// Controller that implements dragging events to handle the scroll view and scroll bar event.
        /// </summary>
        private sealed class ScrollController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler,
            IBeginDragHandler,
            IEndDragHandler
        {
            public bool interactable = true;
            public SheetScroller parent;

            public void OnPointerDown(PointerEventData _)
            {
                if (interactable && parent.Interactable)
                    parent.OnPointerDown(this); // If interactable signal an event from this controller.
            }

            public void OnBeginDrag(PointerEventData _)
            {
                if (interactable && parent.Interactable) parent.OnBeginDrag(); // If interactable signal an event from this controller.
            }

            public void OnPointerUp(PointerEventData _)
            {
                if (interactable && parent.Interactable) parent.OnPointerUp(); // If interactable signal an event from this controller.
            }

            public void OnEndDrag(PointerEventData _)
            {
                if (interactable && parent.Interactable) parent.OnEndDrag(); // If interactable signal an event from this controller.
            }
        }

        /// <summary>Indicates if the one of the controls is currently dragging.</summary>
        private bool _isDragging;

        /// <summary>If not null, refers to the control currently initiating a drag/dragging.</summary>
        private ScrollController _eventsSource;

        /// <summary>The scroll rect's controller.</summary>
        private ScrollController _scrollRectController;

        /// <summary>The scroll bar's controller.</summary>
        private ScrollController _scrollBarController;

        private void OnPointerDown(ScrollController source) // Mirrors the Unity event of the same name.
        {
            _eventsSource = source; // Keep track of which controller is controlling.

            // Set only the current event source as interactable, to avoid glitchy behavior.
            scrollBar.interactable = _scrollBarController.interactable = _eventsSource == _scrollBarController;
            _scrollRectController.interactable = _eventsSource == _scrollRectController;

            OnBeginScroll();
        }

        private void OnBeginDrag() // Mirrors the Unity event of the same name.
        {
            _isDragging = true; // "Upgrade" from a potential drag action to an actual drag.
        }

        private void OnPointerUp() // Mirrors the Unity event of the same name.
        {
            if (!_isDragging) OnEndScroll(); // If not currently dragging, signal then end of the dragging action.
        }

        private void OnEndDrag() // Mirrors the Unity event of the same name.
        {
            if (_isDragging) OnEndScroll(); // If currently dragging, signal then end of the dragging action.
        }

        /// <summary>Generate a scroll begin event.</summary>
        private void OnBeginScroll() => Controller.StartScroll(ScrollPosition);

        private void Update()
        {
            if (_isDragging) Controller.ScrollUpdate(ScrollPosition); // If dragging then update the scrolling position.
        }

        /// <summary>Generate a scroll end event.</summary>
        private void OnEndScroll()
        {
            _isDragging = false; // No longer dragging.
            _eventsSource = null; // No longer controlled by a controller.

            // Enable all controllers.
            scrollBar.interactable = _scrollBarController.interactable = _scrollRectController.interactable = true;

            Controller.EndScroll(ScrollPosition);
        }

        /// <summary>
        /// Ordered pool controller.
        /// </summary>
        private sealed class SheetSegmentsPoolController : IOrderedPoolController<Image>
        {
            private readonly SheetScroller _parent;

            public SheetSegmentsPoolController(SheetScroller parent) => _parent = parent;

            public Image Instantiate(int index)
            {
                // Instantiate a new sheet segment and name it with it's index.
                var item = Object.Instantiate(_parent.segmentPrefab, _parent.segmentsParent.transform);
                item.rectTransform.sizeDelta = Vector2.zero;
                item.name = $"Segment#{index}";
                return item;
            }

            public void Rewind(Image item)
            {
                // Remove the segment's sprite and hide it by setting it's size to zero.
                item.sprite = null;
                item.rectTransform.sizeDelta = Vector2.zero;
            }
        }

        private void Start()
        {
            // Initialize the sheet segments pool, scrolling controllers and cached parameters.

            _cursorImage = cursor.GetComponent<Image>();

            _sheetSegmentsPool = new OrderedPool<Image>(200, new SheetSegmentsPoolController(this));

            _scrollRectController = scrollRect.AddComponent<ScrollController>();
            _scrollRectController.parent = this;
            _scrollBarController = scrollBar.AddComponent<ScrollController>();
            _scrollBarController.parent = this;

            _cursorPosition = (int)cursor.anchoredPosition.x;
            _segmentsParentTransform = segmentsParent.GetComponent<RectTransform>();

            var viewportRect = GetComponent<RectTransform>().rect;
            _viewportWidth = (int)viewportRect.width;
            _viewportHeight = (int)viewportRect.height;
        }

        public void Load(Graphics graphics, int firstValidPosition, int lastValidPosition)
        {
            _firstValidPosition = firstValidPosition;

            // These actions are needed because othewise the elements gets resized wrong.
            _segmentsParentTransform.sizeDelta = new Vector2(0, 0);
            segmentsParent.padding.left = segmentsParent.padding.right = 0;

            // Load all sheet segments sprites.
            _sheetSegmentsPool.Rewind();
            foreach (var segment in graphics.SheetSegments)
            {
                var istance = _sheetSegmentsPool.Extract();
                var sprite = segment.Sprite;
                istance.sprite = sprite;
                istance.SetNativeSize();
            }

            // Set the horizontal padding to align the content's scroll range to the given valid positions range.
            _sheetOffsetStart = _cursorPosition - _firstValidPosition;
            var offsetEnd = _viewportWidth - (graphics.Width - lastValidPosition) - _cursorPosition;
            segmentsParent.padding.left = _sheetOffsetStart;
            segmentsParent.padding.right = offsetEnd;

            // Resize the segments parent to contain the sheets.
            _segmentsParentTransform.sizeDelta =
                new Vector2(graphics.Width + _sheetOffsetStart + offsetEnd, _viewportHeight);

            // Align the beat cursor vertically with the sheet's staff.
            cursor.anchoredPosition = new Vector2(_cursorPosition, -graphics.StaffPosition);
            cursor.sizeDelta = new Vector2(cursor.sizeDelta.x, graphics.StaffHeight);

            // Update the scrolling visualization.
            OnPositionChange(Controller.Position);
        }

        public void OnPositionChange(int x) =>
            _segmentsParentTransform.anchoredPosition = new Vector2(-x + _cursorPosition - _sheetOffsetStart, 0);
    }
}