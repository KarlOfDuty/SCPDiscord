// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: BotToPlugin/SyncRoleCommand.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace SCPDiscord.Interface {

  /// <summary>Holder for reflection information generated from BotToPlugin/SyncRoleCommand.proto</summary>
  public static partial class SyncRoleCommandReflection {

    #region Descriptor
    /// <summary>File descriptor for BotToPlugin/SyncRoleCommand.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static SyncRoleCommandReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "CiFCb3RUb1BsdWdpbi9TeW5jUm9sZUNvbW1hbmQucHJvdG8SFFNDUERpc2Nv",
            "cmQuSW50ZXJmYWNlIlwKD1N5bmNSb2xlQ29tbWFuZBIRCglDaGFubmVsSUQY",
            "ASABKAQSEQoJRGlzY29yZElEGAIgASgEEg8KB1N0ZWFtSUQYAyABKAQSEgoK",
            "RGlzY29yZFRhZxgEIAEoCWIGcHJvdG8z"));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { },
          new pbr::GeneratedClrTypeInfo(null, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::SCPDiscord.Interface.SyncRoleCommand), global::SCPDiscord.Interface.SyncRoleCommand.Parser, new[]{ "ChannelID", "DiscordID", "SteamID", "DiscordTag" }, null, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  public sealed partial class SyncRoleCommand : pb::IMessage<SyncRoleCommand>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<SyncRoleCommand> _parser = new pb::MessageParser<SyncRoleCommand>(() => new SyncRoleCommand());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<SyncRoleCommand> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::SCPDiscord.Interface.SyncRoleCommandReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public SyncRoleCommand() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public SyncRoleCommand(SyncRoleCommand other) : this() {
      channelID_ = other.channelID_;
      discordID_ = other.discordID_;
      steamID_ = other.steamID_;
      discordTag_ = other.discordTag_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public SyncRoleCommand Clone() {
      return new SyncRoleCommand(this);
    }

    /// <summary>Field number for the "ChannelID" field.</summary>
    public const int ChannelIDFieldNumber = 1;
    private ulong channelID_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public ulong ChannelID {
      get { return channelID_; }
      set {
        channelID_ = value;
      }
    }

    /// <summary>Field number for the "DiscordID" field.</summary>
    public const int DiscordIDFieldNumber = 2;
    private ulong discordID_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public ulong DiscordID {
      get { return discordID_; }
      set {
        discordID_ = value;
      }
    }

    /// <summary>Field number for the "SteamID" field.</summary>
    public const int SteamIDFieldNumber = 3;
    private ulong steamID_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public ulong SteamID {
      get { return steamID_; }
      set {
        steamID_ = value;
      }
    }

    /// <summary>Field number for the "DiscordTag" field.</summary>
    public const int DiscordTagFieldNumber = 4;
    private string discordTag_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string DiscordTag {
      get { return discordTag_; }
      set {
        discordTag_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as SyncRoleCommand);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(SyncRoleCommand other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (ChannelID != other.ChannelID) return false;
      if (DiscordID != other.DiscordID) return false;
      if (SteamID != other.SteamID) return false;
      if (DiscordTag != other.DiscordTag) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (ChannelID != 0UL) hash ^= ChannelID.GetHashCode();
      if (DiscordID != 0UL) hash ^= DiscordID.GetHashCode();
      if (SteamID != 0UL) hash ^= SteamID.GetHashCode();
      if (DiscordTag.Length != 0) hash ^= DiscordTag.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      output.WriteRawMessage(this);
    #else
      if (ChannelID != 0UL) {
        output.WriteRawTag(8);
        output.WriteUInt64(ChannelID);
      }
      if (DiscordID != 0UL) {
        output.WriteRawTag(16);
        output.WriteUInt64(DiscordID);
      }
      if (SteamID != 0UL) {
        output.WriteRawTag(24);
        output.WriteUInt64(SteamID);
      }
      if (DiscordTag.Length != 0) {
        output.WriteRawTag(34);
        output.WriteString(DiscordTag);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      if (ChannelID != 0UL) {
        output.WriteRawTag(8);
        output.WriteUInt64(ChannelID);
      }
      if (DiscordID != 0UL) {
        output.WriteRawTag(16);
        output.WriteUInt64(DiscordID);
      }
      if (SteamID != 0UL) {
        output.WriteRawTag(24);
        output.WriteUInt64(SteamID);
      }
      if (DiscordTag.Length != 0) {
        output.WriteRawTag(34);
        output.WriteString(DiscordTag);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (ChannelID != 0UL) {
        size += 1 + pb::CodedOutputStream.ComputeUInt64Size(ChannelID);
      }
      if (DiscordID != 0UL) {
        size += 1 + pb::CodedOutputStream.ComputeUInt64Size(DiscordID);
      }
      if (SteamID != 0UL) {
        size += 1 + pb::CodedOutputStream.ComputeUInt64Size(SteamID);
      }
      if (DiscordTag.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(DiscordTag);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(SyncRoleCommand other) {
      if (other == null) {
        return;
      }
      if (other.ChannelID != 0UL) {
        ChannelID = other.ChannelID;
      }
      if (other.DiscordID != 0UL) {
        DiscordID = other.DiscordID;
      }
      if (other.SteamID != 0UL) {
        SteamID = other.SteamID;
      }
      if (other.DiscordTag.Length != 0) {
        DiscordTag = other.DiscordTag;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      input.ReadRawMessage(this);
    #else
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 8: {
            ChannelID = input.ReadUInt64();
            break;
          }
          case 16: {
            DiscordID = input.ReadUInt64();
            break;
          }
          case 24: {
            SteamID = input.ReadUInt64();
            break;
          }
          case 34: {
            DiscordTag = input.ReadString();
            break;
          }
        }
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
            break;
          case 8: {
            ChannelID = input.ReadUInt64();
            break;
          }
          case 16: {
            DiscordID = input.ReadUInt64();
            break;
          }
          case 24: {
            SteamID = input.ReadUInt64();
            break;
          }
          case 34: {
            DiscordTag = input.ReadString();
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
