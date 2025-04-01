using Avalonia.Controls;

namespace BiermanTech.ProjectManager.Controls;

public class ScrollSynchronizer
{
    private readonly ScrollViewer _source;
    private readonly ScrollViewer _target;

    public ScrollSynchronizer(ScrollViewer source, ScrollViewer target)
    {
        _source = source;
        _target = target;
        _source.ScrollChanged += SourceScrollChanged;
    }

    private void SourceScrollChanged(object sender, ScrollChangedEventArgs args)
    {
        if (args.ExtentDelta.Y != 0 || args.OffsetDelta.Y != 0)
        {
            _target.Offset = _target.Offset.WithY(_source.Offset.Y);
        }
    }
}