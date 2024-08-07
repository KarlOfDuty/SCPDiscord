// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: BotToPlugin/PlayerInfoCommand.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021, 8981
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace SCPDiscord.Interface {

  /// <summary>Holder for reflection information generated from BotToPlugin/PlayerInfoCommand.proto</summary>
  public static partial class PlayerInfoCommandReflection {

    #region Descriptor
    /// <summary>File descriptor for BotToPlugin/PlayerInfoCommand.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static PlayerInfoCommandReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "CiNCb3RUb1BsdWdpbi9QbGF5ZXJJbmZvQ29tbWFuZC5wcm90bxIUU0NQRGlz",
            "Y29yZC5JbnRlcmZhY2UimgEKEVBsYXllckluZm9Db21tYW5kEhEKCWNoYW5u",
            "ZWxJRBgBIAEoBBIPCgdzdGVhbUlEGAIgASgJEhUKDWludGVyYWN0aW9uSUQY",
            "AyABKAQSFQoNZGlzY29yZFVzZXJJRBgEIAEoBBIaChJkaXNjb3JkRGlzcGxh",
            "eU5hbWUYBSABKAkSFwoPZGlzY29yZFVzZXJuYW1lGAYgASgJYgZwcm90bzM="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { },
          new pbr::GeneratedClrTypeInfo(null, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::SCPDiscord.Interface.PlayerInfoCommand), global::SCPDiscord.Interface.PlayerInfoCommand.Parser, new[]{ "ChannelID", "SteamID", "InteractionID", "DiscordUserID", "DiscordDisplayName", "DiscordUsername" }, null, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  [global::System.Diagnostics.DebuggerDisplayAttribute("{ToString(),nq}")]
  public sealed partial class PlayerInfoCommand : pb::IMessage<PlayerInfoCommand>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<PlayerInfoCommand> _parser = new pb::MessageParser<PlayerInfoCommand>(() => new PlayerInfoCommand());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<PlayerInfoCommand> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::SCPDiscord.Interface.PlayerInfoCommandReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public PlayerInfoCommand() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public PlayerInfoCommand(PlayerInfoCommand other) : this() {
      channelID_ = other.channelID_;
      steamID_ = other.steamID_;
      interactionID_ = other.interactionID_;
      discordUserID_ = other.discordUserID_;
      discordDisplayName_ = other.discordDisplayName_;
      discordUsername_ = other.discordUsername_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public PlayerInfoCommand Clone() {
      return new PlayerInfoCommand(this);
    }

    /// <summary>Field number for the "channelID" field.</summary>
    public const int ChannelIDFieldNumber = 1;
    private ulong channelID_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public ulong ChannelID {
      get { return channelID_; }
      set {
        channelID_ = value;
      }
    }

    /// <summary>Field number for the "steamID" field.</summary>
    public const int SteamIDFieldNumber = 2;
    private string steamID_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public string SteamID {
      get { return steamID_; }
      set {
        steamID_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "interactionID" field.</summary>
    public const int InteractionIDFieldNumber = 3;
    private ulong interactionID_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public ulong InteractionID {
      get { return interactionID_; }
      set {
        interactionID_ = value;
      }
    }

    /// <summary>Field number for the "discordUserID" field.</summary>
    public const int DiscordUserIDFieldNumber = 4;
    private ulong discordUserID_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public ulong DiscordUserID {
      get { return discordUserID_; }
      set {
        discordUserID_ = value;
      }
    }

    /// <summary>Field number for the "discordDisplayName" field.</summary>
    public const int DiscordDisplayNameFieldNumber = 5;
    private string discordDisplayName_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public string DiscordDisplayName {
      get { return discordDisplayName_; }
      set {
        discordDisplayName_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "discordUsername" field.</summary>
    public const int DiscordUsernameFieldNumber = 6;
    private string discordUsername_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public string DiscordUsername {
      get { return discordUsername_; }
      set {
        discordUsername_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as PlayerInfoCommand);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(PlayerInfoCommand other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (ChannelID != other.ChannelID) return false;
      if (SteamID != other.SteamID) return false;
      if (InteractionID != other.InteractionID) return false;
      if (DiscordUserID != other.DiscordUserID) return false;
      if (DiscordDisplayName != other.DiscordDisplayName) return false;
      if (DiscordUsername != other.DiscordUsername) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      if (ChannelID != 0UL) hash ^= ChannelID.GetHashCode();
      if (SteamID.Length != 0) hash ^= SteamID.GetHashCode();
      if (InteractionID != 0UL) hash ^= InteractionID.GetHashCode();
      if (DiscordUserID != 0UL) hash ^= DiscordUserID.GetHashCode();
      if (DiscordDisplayName.Length != 0) hash ^= DiscordDisplayName.GetHashCode();
      if (DiscordUsername.Length != 0) hash ^= DiscordUsername.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void WriteTo(pb::CodedOutputStream output) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      output.WriteRawMessage(this);
    #else
      if (ChannelID != 0UL) {
        output.WriteRawTag(8);
        output.WriteUInt64(ChannelID);
      }
      if (SteamID.Length != 0) {
        output.WriteRawTag(18);
        output.WriteString(SteamID);
      }
      if (InteractionID != 0UL) {
        output.WriteRawTag(24);
        output.WriteUInt64(InteractionID);
      }
      if (DiscordUserID != 0UL) {
        output.WriteRawTag(32);
        output.WriteUInt64(DiscordUserID);
      }
      if (DiscordDisplayName.Length != 0) {
        output.WriteRawTag(42);
        output.WriteString(DiscordDisplayName);
      }
      if (DiscordUsername.Length != 0) {
        output.WriteRawTag(50);
        output.WriteString(DiscordUsername);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      if (ChannelID != 0UL) {
        output.WriteRawTag(8);
        output.WriteUInt64(ChannelID);
      }
      if (SteamID.Length != 0) {
        output.WriteRawTag(18);
        output.WriteString(SteamID);
      }
      if (InteractionID != 0UL) {
        output.WriteRawTag(24);
        output.WriteUInt64(InteractionID);
      }
      if (DiscordUserID != 0UL) {
        output.WriteRawTag(32);
        output.WriteUInt64(DiscordUserID);
      }
      if (DiscordDisplayName.Length != 0) {
        output.WriteRawTag(42);
        output.WriteString(DiscordDisplayName);
      }
      if (DiscordUsername.Length != 0) {
        output.WriteRawTag(50);
        output.WriteString(DiscordUsername);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int CalculateSize() {
      int size = 0;
      if (ChannelID != 0UL) {
        size += 1 + pb::CodedOutputStream.ComputeUInt64Size(ChannelID);
      }
      if (SteamID.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(SteamID);
      }
      if (InteractionID != 0UL) {
        size += 1 + pb::CodedOutputStream.ComputeUInt64Size(InteractionID);
      }
      if (DiscordUserID != 0UL) {
        size += 1 + pb::CodedOutputStream.ComputeUInt64Size(DiscordUserID);
      }
      if (DiscordDisplayName.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(DiscordDisplayName);
      }
      if (DiscordUsername.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(DiscordUsername);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(PlayerInfoCommand other) {
      if (other == null) {
        return;
      }
      if (other.ChannelID != 0UL) {
        ChannelID = other.ChannelID;
      }
      if (other.SteamID.Length != 0) {
        SteamID = other.SteamID;
      }
      if (other.InteractionID != 0UL) {
        InteractionID = other.InteractionID;
      }
      if (other.DiscordUserID != 0UL) {
        DiscordUserID = other.DiscordUserID;
      }
      if (other.DiscordDisplayName.Length != 0) {
        DiscordDisplayName = other.DiscordDisplayName;
      }
      if (other.DiscordUsername.Length != 0) {
        DiscordUsername = other.DiscordUsername;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(pb::CodedInputStream input) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      input.ReadRawMessage(this);
    #else
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
      if ((tag & 7) == 4) {
        // Abort on any end group tag.
        return;
      }
      switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 8: {
            ChannelID = input.ReadUInt64();
            break;
          }
          case 18: {
            SteamID = input.ReadString();
            break;
          }
          case 24: {
            InteractionID = input.ReadUInt64();
            break;
          }
          case 32: {
            DiscordUserID = input.ReadUInt64();
            break;
          }
          case 42: {
            DiscordDisplayName = input.ReadString();
            break;
          }
          case 50: {
            DiscordUsername = input.ReadString();
            break;
          }
        }
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
      if ((tag & 7) == 4) {
        // Abort on any end group tag.
        return;
      }
      switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
            break;
          case 8: {
            ChannelID = input.ReadUInt64();
            break;
          }
          case 18: {
            SteamID = input.ReadString();
            break;
          }
          case 24: {
            InteractionID = input.ReadUInt64();
            break;
          }
          case 32: {
            DiscordUserID = input.ReadUInt64();
            break;
          }
          case 42: {
            DiscordDisplayName = input.ReadString();
            break;
          }
          case 50: {
            DiscordUsername = input.ReadString();
            break;
          }
        }
      }
    }
    #endif

  }

  #endregion

}

#endregion Designer generated code
