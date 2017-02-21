﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Gltf.Serialization
{
    internal sealed partial class Exporter
    {
        private int ExportBuffer(string uri, int byteLength)
        {
            int index = this.buffers.Count;

            this.buffers.Add(new Schema.Buffer
            {
                Uri = uri,
                ByteLength = byteLength,
            });

            return index;
        }

        private int ExportBufferView(int bufferIndex, long byteOffset, long byteLength)
        {
            int index = this.bufferViews.Count;

            this.bufferViews.Add(new Schema.BufferView
            {
                Buffer = bufferIndex,
                ByteOffset = byteOffset,
                ByteLength = byteLength,
                Target = Schema.BufferViewTarget.ARRAY_BUFFER,
            });

            return index;
        }

        private int ExportAccessor(int bufferViewIndex, Schema.AccessorComponentType componentType, int count, Schema.AccessorType type, IEnumerable<object> min, IEnumerable<object> max)
        {
            int index = this.accessors.Count;

            this.accessors.Add(new Schema.Accessor
            {
                BufferView = bufferViewIndex,
                ByteOffset = 0,
                ComponentType = componentType,
                Count = count,
                Type = type,
                Min = min,
                Max = max,
            });

            return index;
        }

        private int ExportData(Schema.AccessorType type, Schema.AccessorComponentType componentType, int componentSize, int count, IEnumerable<object> min, IEnumerable<object> max, int byteLength, Action<BinaryWriter> writeData)
        {
            int accessorIndex;

            if (this.outputBinary)
            {
                // The offset of the data must be aligned to a multiple of the component size.
                this.binaryBodyWriter.Align(componentSize);

                var bufferViewIndex = this.ExportBufferView(0, this.binaryBodyWriter.BaseStream.Position, byteLength);
                accessorIndex = this.ExportAccessor(bufferViewIndex, componentType, count, type, min, max);

                writeData(this.binaryBodyWriter);
            }
            else
            {
                var bufferUri = string.Format("{0}_buffer{1}.bin", this.outputName, this.buffers.Count);
                var bufferIndex = this.ExportBuffer(bufferUri, byteLength);
                var bufferViewIndex = this.ExportBufferView(bufferIndex, 0, byteLength);
                accessorIndex = this.ExportAccessor(bufferViewIndex, componentType, count, type, min, max);

                using (var fileStream = new FileStream(Path.Combine(this.outputDirectory, bufferUri), FileMode.Create))
                using (var binaryWriter = new BinaryWriter(fileStream))
                {
                    writeData(binaryWriter);

                    binaryWriter.Flush();
                    fileStream.Flush();
                }
            }

            return accessorIndex;
        }

        private int ExportData(IEnumerable<ushort> values)
        {
            return this.ExportData(
                Schema.AccessorType.SCALAR,
                Schema.AccessorComponentType.UNSIGNED_SHORT,
                sizeof(ushort),
                values.Count(),
                new object[] { values.Min() },
                new object[] { values.Max() },
                sizeof(ushort) * values.Count(),
                binaryWriter => values.ForEach(value => binaryWriter.Write(value)));
        }

        private int ExportData(IEnumerable<Vector2> values)
        {
            return this.ExportData(
                Schema.AccessorType.VEC2,
                Schema.AccessorComponentType.FLOAT,
                sizeof(float),
                values.Count(),
                new object[] { values.Select(value => value.x).Min(), values.Select(value => value.y).Min() },
                new object[] { values.Select(value => value.x).Max(), values.Select(value => value.y).Max() },
                sizeof(float) * 2 * values.Count(),
                binaryWriter => values.ForEach(value => binaryWriter.Write(value)));
        }

        private int ExportData(IEnumerable<Vector3> values)
        {
            return this.ExportData(
                Schema.AccessorType.VEC3,
                Schema.AccessorComponentType.FLOAT,
                sizeof(float),
                values.Count(),
                new object[] { values.Select(value => value.x).Min(), values.Select(value => value.y).Min(), values.Select(value => value.z).Min() },
                new object[] { values.Select(value => value.x).Max(), values.Select(value => value.y).Max(), values.Select(value => value.z).Max() },
                sizeof(float) * 3 * values.Count(),
                binaryWriter => values.ForEach(value => binaryWriter.Write(value)));
        }

        private int ExportData(IEnumerable<Vector4> values)
        {
            return this.ExportData(
                Schema.AccessorType.VEC4,
                Schema.AccessorComponentType.FLOAT,
                sizeof(float),
                values.Count(),
                new object[] { values.Select(value => value.x).Min(), values.Select(value => value.y).Min(), values.Select(value => value.z).Min(), values.Select(value => value.w).Min() },
                new object[] { values.Select(value => value.x).Max(), values.Select(value => value.y).Max(), values.Select(value => value.z).Max(), values.Select(value => value.w).Max() },
                sizeof(float) * 4 * values.Count(),
                binaryWriter => values.ForEach(value => binaryWriter.Write(value)));
        }

        private int ExportData(IEnumerable<Color> values)
        {
            return this.ExportData(
                Schema.AccessorType.VEC4,
                Schema.AccessorComponentType.FLOAT,
                sizeof(float),
                values.Count(),
                new object[] { values.Select(value => value.r).Min(), values.Select(value => value.g).Min(), values.Select(value => value.b).Min(), values.Select(value => value.a).Min() },
                new object[] { values.Select(value => value.r).Max(), values.Select(value => value.g).Max(), values.Select(value => value.b).Max(), values.Select(value => value.a).Max() },
                sizeof(float) * 4 * values.Count(),
                binaryWriter => values.ForEach(value => binaryWriter.Write(value)));
        }
    }
}