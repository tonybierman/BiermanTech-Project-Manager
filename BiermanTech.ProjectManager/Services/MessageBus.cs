using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace BiermanTech.ProjectManager.Services;

public class MessageBus : IMessageBus
{
    private readonly Subject<object> _subject = new Subject<object>();

    public void Publish<T>(T message)
    {
        _subject.OnNext(message);
    }

    public IObservable<T> Subscribe<T>()
    {
        return _subject.OfType<T>();
    }
}