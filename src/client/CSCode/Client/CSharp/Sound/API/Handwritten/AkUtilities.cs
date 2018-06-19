#if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.
public class AkMigrating
{
    public static bool IsMigrating = false;
}


/// <summary>
/// This is based on FNVHash as used by the DataManager
/// to assign short IDs to objects. Be sure to keep them both in sync
/// when making changes!
/// </summary>

public class AkShortIDGenerator
{
	static AkShortIDGenerator()
	{
		HashSize = 32;
	}
		
	public static byte HashSize
	{
		get
		{
			return s_hashSize;
		}
			
		set
		{
			s_hashSize = value;
			s_mask = (uint)((1 << s_hashSize) - 1);
		}
	}
		
	public static uint Compute(string in_name)
	{
        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(in_name.ToLower());
			
		// Start with the basis value
		uint hval = s_offsetBasis32;
			
		for (int i = 0; i < buffer.Length; i++)
		{
			// multiply by the 32 bit FNV magic prime mod 2^32
			hval *= s_prime32;
				
			// xor the bottom with the current octet
			hval ^= buffer[i];
		}
			
		if (s_hashSize == 32)
			return hval;
			
		// XOR-Fold to the required number of bits
		return (hval >> s_hashSize) ^ (hval & s_mask);
	}
		
	private static byte s_hashSize;
	private static uint s_mask;
		
	private const uint s_prime32 = 16777619;
	private const uint s_offsetBasis32 = 2166136261;
}


#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.