using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzlesGame.Views.PuzzleMenu.Components.Scroller
{
    public class LazyScroller : MonoBehaviour
    {
        [SerializeField]
        private ScrollRect _scrollRect;

        [SerializeField]
        private RectTransform _content;

        [SerializeField]
        private GameObject _itemPrefab;

        [SerializeField]
        private int _extraBuffer = 2;

        private ILazyScrollerDataProvider _dataProvider;
        private readonly List<(ILazyScrollerElement element, int index)> _activeItems = new();
        private readonly Stack<ILazyScrollerElement> _pooledItems = new();
        private float _itemHeight;
        private int _visibleItemCount;
        private bool _useExistingObjects;

        private RectTransform _viewport;
        private float _lastScrollPosition = -1f;

        private void Awake()
        {
            _viewport = _scrollRect.viewport;
            
            // Check if we have child objects that implement ILazyScrollerElement
            _useExistingObjects = false;
            for (int i = 0; i < _content.childCount; i++)
            {
                var child = _content.GetChild(i);
                if (child.TryGetComponent<ILazyScrollerElement>(out _))
                {
                    _useExistingObjects = true;
                    break;
                }
            }
            
            if (_useExistingObjects)
            {
                // Use the first child's height as the item height
                if (_content.childCount > 0)
                {
                    _itemHeight = ((RectTransform)_content.GetChild(0)).rect.height;
                    
                    // Add all existing children to the pool
                    for (int i = 0; i < _content.childCount; i++)
                    {
                        var child = _content.GetChild(i);
                        if (child.TryGetComponent<ILazyScrollerElement>(out var element))
                        {
                            _pooledItems.Push(element);
                            element.OnReturnedToPool();
                        }
                    }
                }
                else
                {
                    Debug.LogError("LazyScroller is set to use existing objects but has no children in content.");
                }
            }
            else if (_itemPrefab != null)
            {
                _itemHeight = ((RectTransform)_itemPrefab.transform).rect.height;
            }
            else
            {
                Debug.LogError("LazyScroller requires either a prefab or existing child objects that implement ILazyScrollerElement.");
            }
        }

        private void Update()
        {
            if (Mathf.Abs(_scrollRect.verticalNormalizedPosition - _lastScrollPosition) > Mathf.Epsilon)
            {
                _lastScrollPosition = _scrollRect.verticalNormalizedPosition;
                UpdateVisibleItems();
            }
        }

        public void SetData<T>(IReadOnlyList<T> newData)
        {
            _dataProvider = new LazyScrollerDataProvider<T>(newData);
            _content.sizeDelta = new Vector2(_content.sizeDelta.x, _dataProvider.GetDataSize() * _itemHeight);

            foreach (var item in _activeItems)
            {
                _pooledItems.Push(item.element);
                item.element.OnReturnedToPool();
            }

            _activeItems.Clear();
            UpdateVisibleItems(forceUpdate: true);
        }

        private void UpdateVisibleItems(bool forceUpdate = false)
        {
            if (_dataProvider == null)
                return;
            var scrollY = _content.anchoredPosition.y;
            var firstIndex = Mathf.Max(0, Mathf.FloorToInt(scrollY / _itemHeight) - _extraBuffer);
            var lastIndex = Mathf.Min(_dataProvider.GetDataSize() - 1, firstIndex + GetVisibleItemCount() + _extraBuffer);

            // Recycle items not in view
            for (var i = _activeItems.Count - 1; i >= 0; i--)
            {
                var activeItem = _activeItems[i];
                if (activeItem.index < firstIndex || activeItem.index > lastIndex)
                {
                    _pooledItems.Push(activeItem.element);
                    activeItem.element.OnReturnedToPool();
                    _activeItems.RemoveAt(i);
                }
            }

            // Add new visible items
            for (var i = firstIndex; i <= lastIndex; i++)
            {
                if (_activeItems.Exists(item => item.index == i)) continue;

                var item = GetItem();
                if (item == null) continue;
                
                item.GetRectTransform().anchoredPosition = new Vector2(0, -i * _itemHeight);
                _dataProvider.FillElement(item, i);
                _activeItems.Add((item, i));
            }
        }

        private int GetVisibleItemCount()
        {
            if (_visibleItemCount == 0)
            {
                _visibleItemCount = Mathf.CeilToInt(_viewport.rect.height / _itemHeight);
            }

            return _visibleItemCount;
        }

        private ILazyScrollerElement GetItem()
        {
            if (_pooledItems.Count > 0)
            {
                var item = _pooledItems.Pop();
                item.OnTakenFromPool();
                return item;
            }

            if (_itemPrefab != null)
            {
                var go = Instantiate(_itemPrefab, _content);
                var poolable = go.GetComponent<ILazyScrollerElement>();
                if (poolable == null)
                {
                    Debug.LogError("Item prefab must have a component that implements ILazyScrollerElement.");
                }

                return poolable;
            }
            
            Debug.LogError("LazyScroller is not properly configured.");
            return null;
        }
    }
}