using DarkRift;
using Unity.Collections;
using UnityEngine;
using static SerializableDarkRift;
public static class BasisNetworkAvatarDecompressor
{
    public static void DeCompress(BasisNetworkSendBase Base, ServerSideSyncPlayerMessage ServerSideSyncPlayerMessage)
    {
        Base.LASM = ServerSideSyncPlayerMessage.avatarSerialization;
        DecompressAvatar(ref Base.Target, Base.LASM.array, Base.PositionRanged, Base.ScaleRanged);
    }
    public static void DeCompress(BasisNetworkSendBase Base, LocalAvatarSyncMessage ServerSideSyncPlayerMessage)
    {
        Base.LASM = ServerSideSyncPlayerMessage;
        DecompressAvatar(ref Base.Target, Base.LASM.array, Base.PositionRanged, Base.ScaleRanged);
    }
    public static void DecompressAvatar(ref BasisAvatarData AvatarData, byte[] AvatarUpdate, BasisRangedFloatData PositionRanged, BasisRangedFloatData ScaleRanged)
    {
        DecompressAvatarUpdate(AvatarUpdate, out Vector3 PlayerPosition, out Vector3 Scale, out Vector3 BodyPosition, out Quaternion Rotation, ref AvatarData.Muscles, PositionRanged, ScaleRanged);
        AvatarData.Vectors[1] = BodyPosition;
        AvatarData.Vectors[0] = PlayerPosition;
        AvatarData.Vectors[2] = Scale;
        AvatarData.Quaternions[0] = Rotation;
    }
    public static void DecompressAvatarUpdate(byte[] compressedData, out Vector3 NewPosition, out Vector3 Scale, out Vector3 BodyPosition, out Quaternion Rotation, ref NativeArray<float> muscles, BasisRangedFloatData PositionRanged, BasisRangedFloatData ScaleRanged)
    {
        if (compressedData != null && compressedData.Length != 0)
        {
            using (var bitPacker = DarkRiftReader.CreateFromArray(compressedData, 0, compressedData.Length))
            {
                DecompressScaleAndPosition(bitPacker, out NewPosition, out BodyPosition, out Scale, PositionRanged, ScaleRanged);
                BasisCompressionOfRotation.DecompressQuaternion(bitPacker, out Rotation);
                BasisCompressionOfMuscles.DecompressMuscles(bitPacker, ref muscles);
            }
        }
        else
        {
            Debug.LogError("Array was null or empty!");
            NewPosition = new Vector3();
            Scale = new Vector3();
            BodyPosition = new Vector3();
            Rotation = new Quaternion();
        }
    }
    public static void DecompressScaleAndPosition(DarkRiftReader Packer, out Vector3 Position, out Vector3 BodyPosition, out Vector3 Scale, BasisRangedFloatData PositionRanged, BasisRangedFloatData ScaleRanged)
    {
        Position = BasisCompressionOfPosition.DecompressVector3(Packer, PositionRanged);
        BodyPosition = BasisCompressionOfPosition.DecompressVector3(Packer, PositionRanged);

        Scale = BasisCompressionOfPosition.DecompressVector3(Packer, ScaleRanged);
    }
}