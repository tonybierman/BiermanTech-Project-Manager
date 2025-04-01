using System;

namespace BiermanTech.ProjectManager.Services;

public interface IMessageBus
{
    void Publish<T>(T message);
    IObservable<T> Subscribe<T>();
}