using Android.Content.Res;
using Android.OS;
using LettreForAndroid.Class;
using System.Collections.Generic;
using System.IO;
using TFIDF;

namespace LettreForAndroid.Utility
{
	class PredictionEngine : MainActivity
	{
		TfIdf tfidf;

		public PredictionEngine()
		{
			// 문서 유사도 분석을 위한 TfIdf
			tfidf = new TfIdf();

			// 미리 n그램으로 분리된 문서를 불러와서 예측하는데 걸리는 연산시간을 단축
			tfidf.Load_documents(this.Assets.Open("msg_non_ratio_4ngram_trainset.csv"));
		}

		public Dictionary<string, int[]> Predict(DialogueSet dialogueSet)
		{
			//string 연락처  int[] 7개 카테고리의 레이블 수
			Dictionary<string, int[]> receivedDatas = new Dictionary<string, int[]>();

			// dialogueSet에 있는 전화번호와 문자메시지를 


			foreach (var elem in dialogueSet.DialogueList.Values)
			{
				int[] receive_labels = new int[Dialogue.Lable_COUNT];

				foreach (var textMessage in elem.TextMessageList)
				{
					List<Similarity> sim = tfidf.Similarities(DataEmbedding.to_ngram(textMessage.Msg, 4));

					Similarity max = new Similarity(7, 0);

					foreach (var s in sim)
					{
						if (max.similarity < s.similarity)
						{
							max.label = s.label;
							max.similarity = s.similarity;
						}
					}
					receive_labels[max.label - 1]++;
				}
				receivedDatas.Add(elem.Address, receive_labels);
			}
			return receivedDatas;
		}
	}
}