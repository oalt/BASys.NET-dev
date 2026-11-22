using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BASys.Views.Dialogs
{
    public partial class EnumRadioGroup<TEnum> where TEnum : struct, Enum
    {
        private readonly string _groupName = Guid.NewGuid().ToString("N");

        [Parameter, EditorRequired]
        public IEnumerable<TEnum> Values { get; set; } = Array.Empty<TEnum>();

        [Parameter]
        public TEnum Value { get; set; }

        [Parameter]
        public EventCallback<TEnum> ValueChanged { get; set; }

        [Parameter, EditorRequired]
        public Func<TEnum, string> TextProvider { get; set; } = default!;

        private string GetText(TEnum value)
        {
            string result = TextProvider(value);

            return result;
        }

        private async Task OnChanged(ChangeEventArgs e)
        {
            TEnum newValue;

            if (Enum.TryParse<TEnum>(e.Value?.ToString(), out newValue))
            {
                await ValueChanged.InvokeAsync(newValue);
            }
        }
    }
}
