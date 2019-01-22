﻿using System.Collections.Generic;
using System.IO;

namespace MotionList
{
    public class MotionFile
    {
        public const ulong Magic = 0x06f5fea1e8;//motion

        public ulong IDHash { get; set; }
        public List<Motion> Entries { get; set; }

        public MotionFile(string filename)
        {
            Entries = new List<Motion>();
            using (BinaryReader reader = new BinaryReader(File.OpenRead(filename)))
            {
                if (reader.ReadUInt64() != Magic)
                    throw new InvalidDataException("File contains an invalid header");
                IDHash = reader.ReadUInt64();
                int count = reader.ReadInt32();
                reader.BaseStream.Position = 0x18;//alignment
                for (int i = 0; i < count; i++)
                {
                    ulong motionKind = reader.ReadUInt64();
                    ulong gameHash = reader.ReadUInt64();
                    ushort flags = reader.ReadUInt16();
                    byte frames = reader.ReadByte();
                    bool hasAnimation = reader.ReadBoolean();
                    int unk14 = reader.ReadInt32();
                    if (hasAnimation)
                        Entries.Add(new MotionA(reader, motionKind, gameHash, flags, frames, hasAnimation, unk14));
                    else
                        Entries.Add(new MotionB(reader, motionKind, gameHash, flags, frames, hasAnimation, unk14));
                }
            }
        }

        public void Save(string filename)
        {
            using (BinaryWriter writer = new BinaryWriter(File.Create(filename)))
            {
                writer.Write(Magic);
                writer.Write(IDHash);
                writer.Write((long)Entries.Count);
                foreach (Motion motion in Entries)
                    motion.Write(writer);
            }
        }
    }
}
