using Content.Client.UserInterface.Controls;
using Content.Shared.Radio.Components;
using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Client.Radio.Ui;

[GenerateTypedNameReferences]
public sealed partial class IntercomMenu : FancyWindow
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    public event Action<bool>? OnMicPressed;
    public event Action<bool>? OnSpeakerPressed;
    public event Action<string>? OnChannelSelected;

    private readonly List<string> _channels = new();

    public IntercomMenu(Entity<IntercomComponent> entity)
    {
        RobustXamlLoader.Load(this);
        IoCManager.InjectDependencies(this);

        MicButton.OnPressed += args => OnMicPressed?.Invoke(args.Button.Pressed);
        SpeakerButton.OnPressed += args => OnSpeakerPressed?.Invoke(args.Button.Pressed);

        Update(entity);
    }

    public void Update(Entity<IntercomComponent> entity)
    {
        MicButton.Pressed = entity.Comp.MicrophoneEnabled;
        SpeakerButton.Pressed = entity.Comp.SpeakerEnabled;

        MicButton.Disabled = entity.Comp.SupportedChannels.Count == 0;
        SpeakerButton.Disabled = entity.Comp.SupportedChannels.Count == 0;
        ChannelOptions.Disabled = entity.Comp.SupportedChannels.Count == 0;

        ChannelOptions.Clear();
        _channels.Clear();
        for (var i = 0; i < entity.Comp.SupportedChannels.Count; i++)
        {
            var channel = entity.Comp.SupportedChannels[i];
            if (!_prototype.TryIndex(channel, out var prototype))
                continue;

            _channels.Add(channel);
            ChannelOptions.AddItem(Loc.GetString(prototype.Name), i);

            if (channel == entity.Comp.CurrentChannel)
                ChannelOptions.Select(i);
        }

        if (entity.Comp.SupportedChannels.Count == 0)
        {
            ChannelOptions.AddItem(Loc.GetString("intercom-options-none"), 0);
            ChannelOptions.Select(0);
        }

        ChannelOptions.OnItemSelected += args =>
        {
            if (!_channels.TryGetValue(args.Id, out var proto))
                return;

            ChannelOptions.SelectId(args.Id);
            OnChannelSelected?.Invoke(proto);
        };
    }
}

