using OpenBveApi.Colors;

namespace OpenBveApi.Runtime
{
	/// <summary>Plays a sound.</summary>
	/// <param name="index">The index to the sound to be played.</param>
	/// <param name="volume">The initial volume of the sound. A value of 1.0 represents nominal volume.</param>
	/// <param name="pitch">The initial pitch of the sound. A value of 1.0 represents nominal pitch.</param>
	/// <param name="looped">Whether the sound should be played in an indefinate loop.</param>
	/// <returns>The handle to the sound, or a null reference if the sound could not be played.</returns>
	/// <exception cref="System.InvalidOperationException">Raised when the host application does not allow the function to be called.</exception>
	public delegate SoundHandle PlaySoundDelegate(int index, double volume, double pitch, bool looped);

	/// <summary>Plays a sound.</summary>
	/// <param name="index">The index to the sound to be played.</param>
	/// <param name="volume">The initial volume of the sound. A value of 1.0 represents nominal volume.</param>
	/// <param name="pitch">The initial pitch of the sound. A value of 1.0 represents nominal pitch.</param>
	/// <param name="looped">Whether the sound should be played in an indefinate loop.</param>
	/// <param name="carIndex">The index of the car this sound is to be attached to</param>
	/// <returns>The handle to the sound, or a null reference if the sound could not be played.</returns>
	/// <exception cref="System.InvalidOperationException">Raised when the host application does not allow the function to be called.</exception>
	public delegate SoundHandle PlayCarSoundDelegate(int index, double volume, double pitch, bool looped, int carIndex);

	/// <summary>Adds a message to the in-game display</summary>
	/// <param name="Message">The message to display</param>
	/// <param name="Color">The color in which to display the message</param>
	/// <param name="Time">The time in seconds for which to display the message</param>
	public delegate void AddInterfaceMessageDelegate(string Message, MessageColor Color, double Time);

	/// <summary>Adds a score to the after game log</summary>
	/// <param name="Score">The score to add or subtract</param>
	/// <param name="Message">The message to be displayed in the post-game log</param>
	/// /// <param name="Color">The color in which to display the message</param>
	/// <param name="Time">The time in seconds for which to display the message</param>
	public delegate void AddScoreDelegate(int Score, string Message, MessageColor Color, double Time);
}
