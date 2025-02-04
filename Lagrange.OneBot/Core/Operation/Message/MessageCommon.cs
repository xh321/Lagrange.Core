using System.Reflection;
using System.Text.Json;
using Lagrange.Core.Message;
using Lagrange.Core.Utility.Extension;
using Lagrange.OneBot.Core.Entity.Action;
using Lagrange.OneBot.Core.Entity.Message;
using Lagrange.OneBot.Core.Message;

namespace Lagrange.OneBot.Core.Operation.Message;

public static class MessageCommon
{
    private static readonly Dictionary<string, ISegment> TypeToSegment;
    
    static MessageCommon()
    {
        TypeToSegment = new Dictionary<string, ISegment>();
        
        foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
        {
            var attribute = type.GetCustomAttribute<SegmentSubscriberAttribute>();
            if (attribute != null) TypeToSegment[attribute.SendType] = (ISegment)type.CreateInstance(false);
        }
    }
    
    public static MessageBuilder ParseChain(OneBotMessage message)
    {
        var builder = message.MessageType == "private"
            ? MessageBuilder.Friend(message.UserId ?? 0)
            : MessageBuilder.Group(message.GroupId ?? 0);
        BuildMessages(builder, message.Messages);

        return builder;
    }
    
    public static MessageBuilder ParseChain(OneBotMessageSimple message)
    {
        var builder = message.MessageType == "private"
            ? MessageBuilder.Friend(message.UserId ?? 0)
            : MessageBuilder.Group(message.GroupId ?? 0);
        BuildMessages(builder, message.Messages);

        return builder;
    }
    
    public static MessageBuilder ParseChain(OneBotMessageText message)
    {
        var builder = message.MessageType == "private"
            ? MessageBuilder.Friend(message.UserId ?? 0)
            : MessageBuilder.Group(message.GroupId ?? 0);
        builder.Text(message.Messages);

        return builder;
    }
    
    public static MessageBuilder ParseChain(OneBotPrivateMessage message)
    {
        var builder = MessageBuilder.Friend(message.UserId);
        BuildMessages(builder, message.Messages);

        return builder;
    }
    
    public static MessageBuilder ParseChain(OneBotPrivateMessageSimple message)
    {
        var builder = MessageBuilder.Friend(message.UserId);
        BuildMessages(builder, message.Messages);

        return builder;
    }
    
    public static MessageBuilder ParseChain(OneBotPrivateMessageText message)
    {
        var builder = MessageBuilder.Group(message.UserId);
        builder.Text(message.Messages);

        return builder;
    }

    
    public static MessageBuilder ParseChain(OneBotGroupMessage message)
    {
        var builder = MessageBuilder.Group(message.GroupId);
        BuildMessages(builder, message.Messages);

        return builder;
    }
    
    public static MessageBuilder ParseChain(OneBotGroupMessageSimple message)
    {
        var builder = MessageBuilder.Group(message.GroupId);
        BuildMessages(builder, message.Messages);

        return builder;
    }
    
    public static MessageBuilder ParseChain(OneBotGroupMessageText message)
    {
        var builder = MessageBuilder.Group(message.GroupId);
        builder.Text(message.Messages);

        return builder;
    }

    private static void BuildMessages(MessageBuilder builder, List<OneBotSegment> segments)
    {
        foreach (var segment in segments)
        {
            if (TypeToSegment.TryGetValue(segment.Type, out var instance))
            {
                var cast = (ISegment)((JsonElement)segment.Data).Deserialize(instance.GetType())!;
                instance.Build(builder, cast);
            }
        }
    }
    
    private static void BuildMessages(MessageBuilder builder, OneBotSegment segment)
    {
            if (TypeToSegment.TryGetValue(segment.Type, out var instance))
            {
                var cast = (ISegment)((JsonElement)segment.Data).Deserialize(instance.GetType())!;
                instance.Build(builder, cast);
            }
    }
}