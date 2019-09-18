using LettreForAndroid.Class;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TFIDF;

namespace LettreForAndroid.Utility
{
	class PredictionEngine
	{
		TfIdf tfidf;

		public PredictionEngine()
		{
			// 문서 유사도 분석을 위한 TfIdf
			tfidf = new TfIdf();


			// Assets 폴더내 파일 읽기
			var filename = Android.App.Application.Context.Assets.Open("msg_non_ratio_4ngram_trainset.csv");

			// 미리 n그램으로 분리된 문서를 불러와서 예측하는데 걸리는 연산시간을 단축
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

			tfidf.Load_documents(filename,Encoding.GetEncoding("euc-kr"));

		}
			
		public Dictionary<string, int[]> Predict(DialogueSet dialogueSet)
		{
			//string 연락처  int[] 7개 카테고리의 레이블 수
			Dictionary<string, int[]> receivedDatas = new Dictionary<string, int[]>();
			DataEmbedding dataEmbedding = new DataEmbedding();

			// dialogueSet에 있는 전화번호와 문자메시지에서 유사도를 측정
			foreach (var elem in dialogueSet.DialogueList.Values)
			{
				int[] receive_labels = new int[Dialogue.Lable_COUNT];

				foreach (var textMessage in elem.TextMessageList)
				{
					//문자메시지에 대해 훈련된 문서로 유사도를 분석

					Similarity[] sim = tfidf.Similarities(dataEmbedding.to_ngram(dataEmbedding.del_digit(textMessage.Msg), 4));

					//가장 높은 유사도를 가지는 문서의 레이블을 가져옴
					Similarity maxObj;
					if (sim.Length > 1)
						maxObj = sim.Aggregate((i1, i2) => i1.similarity > i2.similarity ? i1 : i2);
					else if (sim.Length == 1)
						maxObj = new Similarity(sim[0].label, sim[0].similarity);
					else
						maxObj = new Similarity(7, 0);

					//System.Diagnostics.Debug.WriteLine("유사도 : " + maxObj.similarity + " 레이블 : " + maxObj.label);
					receive_labels[maxObj.label - 1]++;
				}
				receivedDatas.Add(elem.Address, receive_labels);
			}
			return receivedDatas;
		}
	}
}