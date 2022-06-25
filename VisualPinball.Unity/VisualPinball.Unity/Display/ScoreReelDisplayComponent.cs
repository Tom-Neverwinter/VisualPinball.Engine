﻿// Visual Pinball Engine
// Copyright (C) 2022 freezy and VPE Team
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

// ReSharper disable InconsistentNaming

using System;
using System.Collections;
using UnityEngine;

namespace VisualPinball.Unity
{
	[AddComponentMenu("Visual Pinball/Display/Score Reel")]
	public class ScoreReelDisplayComponent : DisplayComponent
	{
		public override string Id { get => _id; set => _id = value; }

		[Unit("positions/s")]
		[Tooltip("Positions per second")]
		public float Speed = 15;

		[Unit("ms")]
		[Tooltip("Wait between positions in milliseconds")]
		public float Wait = 30;

		[Tooltip("The reel components, from left to right.")]
		public ScoreReelComponent[] ReelObjects;

		[SerializeField] public string _id = "display0";

		private int score = 0;


		private void Start()
		{
			foreach (var reelObject in ReelObjects) {
				reelObject.Speed = Speed;
				reelObject.Wait = Wait;
			}
		}

		public override void UpdateFrame(DisplayFrameFormat format, byte[] data)
		{
			score = (int)BitConverter.ToSingle(data);

			var digits = DigitArr(score);
			var j = digits.Length - 1;
			for (var i = ReelObjects.Length - 1; i >= 0; i--) {
				if (j < 0) {
					SetReel(ReelObjects[i], 0);
					j--;
					continue;
				}
				SetReel(ReelObjects[i], digits[j]);
				j--;
			}
		}

		public override void ClearFrame(ClearDisplayData data)
		{
		   StartCoroutine(Reset(data));
		}

		public override void Clear()
		{
		}

		private IEnumerator Reset(ClearDisplayData data)
		{
			data.OnStarted?.Invoke(this, EventArgs.Empty);

			while (score > 0)
			{
				Debug.Log("Waiting...");

				yield return new WaitForSeconds((float)(.750 / 6));

				float newScore = (float)Advance(score);

				Debug.Log($"Updating to {newScore}");

				UpdateFrame(DisplayFrameFormat.Numeric, BitConverter.GetBytes(newScore));

				data.OnPulse?.Invoke(this, EventArgs.Empty);
			}

			data.OnStopped?.Invoke(this, EventArgs.Empty);
		}

		private static void SetReel(ScoreReelComponent sr, int num)
		{
			sr.AnimateTo(num);
		}

		private static int NumDigits(int n) {
			if (n < 0) {
				n = n == int.MinValue ? int.MaxValue : -n;
			}
			return n switch {
				< 10 => 1,
				< 100 => 2,
				< 1000 => 3,
				< 10000 => 4,
				< 100000 => 5,
				< 1000000 => 6,
				< 10000000 => 7,
				< 100000000 => 8,
				< 1000000000 => 9,
				_ => 10
			};
		}

		private static int[] DigitArr(int n)
		{
			var result = new int[NumDigits(n)];
			for (var i = result.Length - 1; i >= 0; i--) {
				result[i] = n % 10;
				n /= 10;
			}
			return result;
		}

		private static int Advance(int score)
		{
			var value = 0;

			foreach (var i in DigitArr(score)) {
				value = (value * 10) + ((i > 0 && i < 9) ? (i + 1) : 0);
			}

			return value;
		}

		#region Unused

		protected override Material CreateMaterial()
		{
			throw new NotImplementedException();
		}
		public override void UpdateDimensions(int width, int height, bool flipX = false)
		{
			Debug.Log($"Reel of {width} requested.");
		}

		public override Color LitColor { get; set; }
		public override Color UnlitColor { get; set; }
		protected override float MeshWidth { get; }
		public override float MeshHeight { get; }
		protected override float MeshDepth { get; }
		public override float AspectRatio { get; set; }

		#endregion
	}
}
