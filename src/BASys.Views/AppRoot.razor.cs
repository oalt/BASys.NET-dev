using BASys.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BASys.Views
{
    public partial class AppRoot
    {
        [Inject]
        private IErrorLogService ErrorLog { get; set; } = default!;

        [Inject]
        private DragDropState DragState { get; set; } = default!;

        [Inject]
        private IJSRuntime JsRuntime { get; set; } = default!;

        private DotNetObjectReference<DragDropState>? _dragDropReference;

        private LoggingErrorBoundary? _boundary;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                _dragDropReference = DotNetObjectReference.Create(DragState);
                await JsRuntime.InvokeVoidAsync("basysDragDrop.initialize", _dragDropReference);
            }
        }

        private void OnRecover()
        {
            _boundary?.Recover();
        }
    }
}
