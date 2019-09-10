﻿using Android.Content.Res;
using Android.OS;
using LettreForAndroid.Class;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
			Stopwatch sw = new Stopwatch();

			sw.Start();

			// 미리 n그램으로 분리된 문서를 불러와서 예측하는데 걸리는 연산시간을 단축
			tfidf.Load_documents(filename);

			sw.Stop();
			System.Diagnostics.Debug.WriteLine("TFIDF 훈련 시간 : " + sw.ElapsedMilliseconds.ToString() + "ms");

		}

		public Dictionary<string, int[]> Predict(DialogueSet dialogueSet)
		{
			//string 연락처  int[] 7개 카테고리의 레이블 수
			Dictionary<string, int[]> receivedDatas = new Dictionary<string, int[]>();
			Regex regex = new Regex("[^a-zA-Zㄱ-ㅎㅏ-ㅣ가-힣]");
			Stopwatch sw = new Stopwatch();
			Stopwatch maxsw = new Stopwatch();
			Stopwatch dialsw = new Stopwatch();


			// dialogueSet에 있는 전화번호와 문자메시지에서 유사도를 측정
			foreach (var elem in dialogueSet.DialogueList.Values)
			{
				int[] receive_labels = new int[Dialogue.Lable_COUNT];
				dialsw.Start();

				foreach (var textMessage in elem.TextMessageList)
				{
					sw.Start();
					var del_digit_msg = regex.Replace(textMessage.Msg, "");
					sw.Stop();
					System.Diagnostics.Debug.WriteLine("숫자제거 시간  : " + sw.ElapsedMilliseconds.ToString() + "ms\n");

					sw.Reset();

					sw.Start();
					Similarity[] sim = tfidf.Similarities(
																				DataEmbedding.to_ngram(
																															del_digit_msg,   //text
																															4));                    //ngram size 

					sw.Stop();
					System.Diagnostics.Debug.WriteLine("내용 : " + textMessage.Msg);
					System.Diagnostics.Debug.WriteLine("similarities 계산 시간 : " + sw.ElapsedMilliseconds.ToString() + "ms\n");
					sw.Reset();
					//sim.Sort(delegate (Similarity A, Similarity B)
					//{
					//	if (A.similarity > B.similarity) return 1;
					//	else if (A.similarity < B.similarity) return -1;
					//	else return 0;
					//});

					//int result = sim[sim.Count - 1].label;
					//receive_labels[result - 1]++;

					maxsw.Start();

					Similarity maxObj;
					if (sim.Length > 1)
						maxObj = sim.Aggregate((i1, i2) => i1.similarity > i2.similarity ? i1 : i2);
					else if (sim.Length == 1)
						maxObj = new Similarity(sim[0].label, sim[0].similarity);
					else
						maxObj = new Similarity(7, 0);

					receive_labels[maxObj.label - 1]++;

					maxsw.Stop();
					System.Diagnostics.Debug.WriteLine("max 계산 시간  : " + maxsw.ElapsedMilliseconds.ToString() + "ms\n");
					maxsw.Reset();
					//Similarity max = new Similarity(7, 0);

					//foreach (var s in sim)
					//{
					//	if (max.similarity < s.similarity)
					//	{
					//		max.label = s.label;
					//		max.similarity = s.similarity;
					//	}
					//}
					//receive_labels[max.label - 1]++;

				}

				dialsw.Stop();
				System.Diagnostics.Debug.WriteLine("다이얼로그 계산시간 : " + dialsw.ElapsedMilliseconds.ToString() + "ms\n");
				dialsw.Reset();
				receivedDatas.Add(elem.Address, receive_labels);

			}

			return receivedDatas;
		}
	}
}