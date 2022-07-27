﻿namespace DaLion.Stardew.Professions.Framework.Events.Ultimate;

#region using directives

using Common.Events;
using System;

#endregion using directives

/// <summary>A dynamic event raised when a <see cref="Ultimates.IUltimate"> is activated.</summary>
internal sealed class UltimateActivatedEvent : ManagedEvent
{
    private readonly Action<object?, IUltimateActivatedEventArgs> _OnActivatedImpl;

    /// <summary>Construct an instance.</summary>
    /// <param name="callback">The delegate to run when the event is raised.</param>
    /// <param name="alwaysHooked">Whether the event should be allowed to override the <c>hooked</c> flag.</param>
    internal UltimateActivatedEvent(Action<object?, IUltimateActivatedEventArgs> callback, bool alwaysHooked = false)
        : base(ModEntry.EventManager)
    {
        _OnActivatedImpl = callback;
        AlwaysHooked = alwaysHooked;
    }

    /// <summary>Raised when a player activates the combat <see cref="Ultimate.IUltimate"/>.</summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    internal void OnActivated(object? sender, IUltimateActivatedEventArgs e)
    {
        if (IsHooked) _OnActivatedImpl(sender, e);
    }
}