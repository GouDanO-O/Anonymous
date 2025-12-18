using UnityEngine.EventSystems;

namespace Core.Game.View.Interface
{
    /// <summary>
    /// UI可以进行拖拽
    /// </summary>
    public interface IUIViewDraggable : IBeginDragHandler, IDragHandler, IEndDragHandler,
        IPointerDownHandler, IPointerUpHandler
    {
        
    }
}