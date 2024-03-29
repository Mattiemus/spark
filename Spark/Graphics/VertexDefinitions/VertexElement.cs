﻿namespace Spark.Graphics
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    
    using Content;

    /// <summary>
    /// Represents a vertex element in a vertex buffer.
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexElement : IEquatable<VertexElement>, IPrimitiveValue
    {
        /// <summary>
        /// Semantic name that this element is bound to.
        /// </summary>
        public VertexSemantic SemanticName;

        /// <summary>
        /// Semantic index this element is bound to.
        /// </summary>
        public int SemanticIndex;

        /// <summary>
        /// Format of this element.
        /// </summary>
        public VertexFormat Format;

        /// <summary>
        /// Offset in bytes from the start of the vertex data.
        /// </summary>
        public int Offset;

        /// <summary>
        /// Initializes a new instance of the <see cref="VertexElement"/> struct.
        /// </summary>
        /// <param name="semantic">The element's shader semantic.</param>
        /// <param name="semanticIndex">The element's shader semantic index</param>
        /// <param name="format">The element's format.</param>
        /// <param name="offset">The element's offset from the start of the vertex data.</param>
        public VertexElement(VertexSemantic semantic, int semanticIndex, VertexFormat format, int offset)
        {
            SemanticName = semantic;
            SemanticIndex = semanticIndex;
            Format = format;
            Offset = offset;
        }

        /// <summary>
        /// Tests equality between two vertex elements.
        /// </summary>
        /// <param name="a">First vertex element</param>
        /// <param name="b">Second vertex element</param>
        /// <returns>True if both are equal, false otherwise.</returns>
        public static bool operator ==(VertexElement a, VertexElement b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// Tests inequality between two vertex elements.
        /// </summary>
        /// <param name="a">First vertex element</param>
        /// <param name="b">Second vertex element</param>
        /// <returns>True if both are not equal, false otherwise.</returns>
        public static bool operator !=(VertexElement a, VertexElement b)
        {
            return !a.Equals(b);
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="obj">Another object to compare to.</param>
        /// <returns>True if <paramref name="obj" /> and this instance are the same type and represent the same value; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            if (obj is VertexElement)
            {
                VertexElement other = (VertexElement)obj;
                return Equals(other);
            }

            return false;
        }

        /// <summary>
        /// Tests equality between this vertex element and another.
        /// </summary>
        /// <param name="other">Other vertex element to compare to</param>
        /// <returns>True if both are equal, false otherwise.</returns>
        public bool Equals(VertexElement other)
        {
            return (SemanticName == other.SemanticName) && (SemanticIndex == other.SemanticIndex) && (Offset == other.Offset) && (Format == other.Format);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 31) + SemanticName.GetHashCode();
                hash = (hash * 31) + SemanticIndex.GetHashCode();
                hash = (hash * 31) + Format.GetHashCode();
                hash = (hash * 31) + Offset.GetHashCode();
                return hash;
            }
        }

        /// <summary>
        /// Returns the fully qualified type name of this instance.
        /// </summary>
        /// <returns>A <see cref="T:System.String" /> containing a fully qualified type name.</returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "SemanticName: {0}, SemanticIndex: {1}, Format: {2}, Offset: {3}", new object[] { SemanticName.ToString(), SemanticIndex.ToString(), Format.ToString(), Offset.ToString() });
        }

        /// <summary>
        /// Writes the primitive data to the output.
        /// </summary>
        /// <param name="output">Primitive writer</param>
        public void Write(IPrimitiveWriter output)
        {
            output.WriteEnum<VertexSemantic>("SemanticName", SemanticName);
            output.Write("SemanticIndex", SemanticIndex);
            output.WriteEnum<VertexFormat>("VertexFormat", Format);
            output.Write("Offset", Offset);
        }

        /// <summary>
        /// Reads the primitive data from the input.
        /// </summary>
        /// <param name="input">Primitive reader</param>
        public void Read(IPrimitiveReader input)
        {
            SemanticName = input.ReadEnum<VertexSemantic>();
            SemanticIndex = input.ReadInt32();
            Format = input.ReadEnum<VertexFormat>();
            Offset = input.ReadInt32();
        }
    }
}
