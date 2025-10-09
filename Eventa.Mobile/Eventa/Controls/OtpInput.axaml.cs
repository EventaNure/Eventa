using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Eventa.Controls;

public partial class OtpInput : UserControl
{
    private List<TextBox> _codeBoxes = [];

    public static readonly StyledProperty<string> CodeProperty = AvaloniaProperty.Register<OtpInput, string>(nameof(Code), defaultValue: string.Empty);

    public string Code
    {
        get => GetValue(CodeProperty);
        set => SetValue(CodeProperty, value);
    }

    public OtpInput()
    {
        InitializeComponent();
        InitializeCodeBoxes();
    }

    private void InitializeCodeBoxes()
    {
        _codeBoxes =
        [
            Code1,
            Code2,
            Code3,
            Code4,
            Code5,
            Code6
        ];

        for (int i = 0; i < _codeBoxes.Count; i++)
        {
            var box = _codeBoxes[i];
            var index = i;

            box.TextChanged += (s, e) => OnCodeBoxTextChanged(index);
            box.KeyDown += (s, e) => OnCodeBoxKeyDown(index, e);
            box.KeyUp += (s, e) => OnCodeBoxKeyUp(index, e);
            box.GotFocus += (s, e) => box.SelectAll();
            box.AddHandler(TextInputEvent, (sender, e) => OnTextInput(index, e), RoutingStrategies.Tunnel);
            box.PastingFromClipboard += async (s, e) => await OnPaste(e);
        }
    }

    private async Task OnPaste(RoutedEventArgs e)
    {
        e.Handled = true; // Prevent default paste behavior

        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        if (clipboard != null)
        {
            var text = await clipboard.GetTextAsync();
            if (!string.IsNullOrEmpty(text))
            {
                HandlePaste(text);
            }
        }
    }

    private void OnTextInput(object sender, TextInputEventArgs e)
    {
        // Check if this is a paste operation (multiple characters)
        if (!string.IsNullOrEmpty(e.Text) && e.Text.Length > 1)
        {
            e.Handled = true; // Prevent default paste behavior
            HandlePaste(e.Text);
        }
    }

    private void HandlePaste(string pastedText)
    {
        // Filter only valid characters (letters and digits)
        var validChars = pastedText.Where(c => char.IsLetterOrDigit(c)).ToArray();

        // Take only up to 6 characters
        var charsToUse = validChars.Take(_codeBoxes.Count).ToArray();

        // Clear all boxes first
        foreach (var box in _codeBoxes)
        {
            box.Text = string.Empty;
        }

        // Fill boxes with valid characters starting from Code1
        for (int i = 0; i < charsToUse.Length; i++)
        {
            _codeBoxes[i].Text = charsToUse[i].ToString();
        }

        // Focus the next empty box or the last box if all filled
        if (charsToUse.Length < _codeBoxes.Count)
        {
            _codeBoxes[charsToUse.Length].Focus();
        }
        else
        {
            _codeBoxes[^1].Focus();
        }

        UpdateCode();
    }

    private void OnCodeBoxTextChanged(int index)
    {
        var box = _codeBoxes[index];

        // Only allow digits and letters
        if (!string.IsNullOrEmpty(box.Text))
        {
            var lastChar = box.Text[^1];
            if (!char.IsLetterOrDigit(lastChar))
            {
                box.Text = string.Empty;
                return;
            }

            // Keep only the last character if multiple were entered
            if (box.Text.Length > 1)
            {
                box.Text = lastChar.ToString();
            }

            // Auto-focus next box
            if (index < _codeBoxes.Count - 1)
            {
                _codeBoxes[index + 1].Focus();
            }
        }

        UpdateCode();
    }

    private void OnCodeBoxKeyDown(int index, KeyEventArgs e)
    {
        // Handle left arrow
        if (e.Key == Key.Left && index > 0)
        {
            _codeBoxes[index - 1].Focus();
        }
        // Handle right arrow
        else if (e.Key == Key.Right && index < _codeBoxes.Count - 1)
        {
            _codeBoxes[index + 1].Focus();
        }
    }

    private void OnCodeBoxKeyUp(int index, KeyEventArgs e)
    {
        // Handle backspace to move to previous box (use KeyUp so text is already cleared)
        if (e.Key == Key.Back && string.IsNullOrEmpty(_codeBoxes[index].Text) && index > 0)
        {
            _codeBoxes[index - 1].Focus();
            _codeBoxes[index - 1].SelectAll();
        }
    }

    private void UpdateCode()
    {
        Code = string.Concat(_codeBoxes.Select(box => box.Text ?? string.Empty));
    }

    public void Clear()
    {
        foreach (var box in _codeBoxes)
        {
            box.Text = string.Empty;
        }
        _codeBoxes[0].Focus();
    }

    public void SetCode(string code)
    {
        if (string.IsNullOrEmpty(code))
        {
            Clear();
            return;
        }

        for (int i = 0; i < _codeBoxes.Count && i < code.Length; i++)
        {
            _codeBoxes[i].Text = code[i].ToString();
        }
    }
}