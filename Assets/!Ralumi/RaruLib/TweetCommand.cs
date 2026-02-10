using UnityEngine;

namespace RaruLib
{
    public class TweetCommand : MonoBehaviour
    {
        private const string ID = "gamelink";

        [SerializeField] private string tag1 = "タイトル";
        [SerializeField] private string tag2 = "";

        public void Tweet(string tweetText)
        {
            if (tag2 != "" && tag1 != "") naichilab.UnityRoomTweet.Tweet(ID, tweetText, tag1, tag2);
            else if (tag1 != "") naichilab.UnityRoomTweet.Tweet(ID, tweetText, tag1);
            else naichilab.UnityRoomTweet.Tweet(ID, tweetText);
        }
    }
}