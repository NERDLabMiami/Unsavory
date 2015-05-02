using UnityEngine;
using System.Collections;

public class Achievements : MonoBehaviour {

	#if UNITY_ANDROID
	public const string TACOS_25 	= "CgkIjOCjtq4PEAIQBQ";
	public const string TACOS_100 	= "CgkIjOCjtq4PEAIQBg";
	public const string TACOS_500 	= "CgkIjOCjtq4PEAIQBw";
	public const string WIPED			= "CgkIjOCjtq4PEAIQAQ";
	public const string ACTIVIST		= "CgkIjOCjtq4PEAIQAg";
	public const string SURVIVED	= "CgkIjOCjtq4PEAIQAw";
	public const string SNEEZED		= "CgkIjOCjtq4PEAIQBA";
	
	#elif UNITY_IOS
	public const string TACOS_25 	= "25tacos";
	public const string TACOS_100 	= "100tacos";
	public const string TACOS_500 	= "500tacos";
	public const string WIPED			= "wiped";
	public const string ACTIVIST		= "activist";
	public const string SURVIVED	= "survived";
	public const string SNEEZED		= "sneezed";

	#else

	public const string TACOS_25 	= "CgkIjOCjtq4PEAIQBQ";
	public const string TACOS_100 	= "CgkIjOCjtq4PEAIQBg";
	public const string TACOS_500 	= "CgkIjOCjtq4PEAIQBw";
	public const string WIPED			= "CgkIjOCjtq4PEAIQAQ";
	public const string ACTIVIST		= "CgkIjOCjtq4PEAIQAg";
	public const string SURVIVED	= "CgkIjOCjtq4PEAIQAw";
	public const string SNEEZED		= "CgkIjOCjtq4PEAIQBA";


	#endif

}
