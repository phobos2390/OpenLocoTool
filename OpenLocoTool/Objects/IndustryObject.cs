﻿using System.ComponentModel;
using System.Linq;
using OpenLocoTool.DatFileParsing;
using OpenLocoTool.Headers;

namespace OpenLocoTool.Objects
{
	[Flags]
	public enum IndustryObjectFlags : uint32_t
	{
		None = 0,
		BuiltInClusters = 1 << 0,
		BuiltOnHighGround = 1 << 1,
		BuiltOnLowGround = 1 << 2,
		BuiltOnSnow = 1 << 3,        // above summer snow line
		BuiltBelowSnowLine = 1 << 4, // below winter snow line
		BuiltOnFlatGround = 1 << 5,
		BuiltNearWater = 1 << 6,
		BuiltAwayFromWater = 1 << 7,
		BuiltOnWater = 1 << 8,
		BuiltNearTown = 1 << 9,
		BuiltAwayFromTown = 1 << 10,
		BuiltNearTrees = 1 << 11,
		BuiltRequiresOpenSpace = 1 << 12,
		Oilfield = 1 << 13,
		Mines = 1 << 14,
		unk15 = 1 << 15,
		CanBeFoundedByPlayer = 1 << 16,
		RequiresAllCargo = 1 << 17,
		unk18 = 1 << 18,
		unk19 = 1 << 19,
		unk20 = 1 << 20,
		HasShadows = 1 << 21,
		unk22 = 1 << 22,
		unk23 = 1 << 23,
		BuiltInDesert = 1 << 24,
		BuiltNearDesert = 1 << 25,
		unk26 = 1 << 26,
		unk27 = 1 << 27,
		unk28 = 1 << 28,
	}

	[TypeConverter(typeof(ExpandableObjectConverter))]
	[LocoStructSize(0x02)]
	[LocoStringCount(0)]
	public record BuildingPartAnimation(
		[property: LocoStructOffset(0x00)] uint8_t numFrames,     // Must be a power of 2 (0 = no part animation, could still have animation sequence)
		[property: LocoStructOffset(0x01)] uint8_t animationSpeed // Also encodes in bit 7 if the animation is position modified
		) : ILocoStruct
	{
		public static int StructSize => 0x2;
	}

	[TypeConverter(typeof(ExpandableObjectConverter))]
	[LocoStructSize(0x02)]
	[LocoStringCount(0)]
	public record IndustryObjectUnk38(
		[property: LocoStructOffset(0x00)] uint8_t var_00,
		[property: LocoStructOffset(0x01)] uint8_t var_01
		) : ILocoStruct
	{
		public static int StructSize => 0x02;
	}

	[TypeConverter(typeof(ExpandableObjectConverter))]
	[LocoStructSize(0x04)]
	[LocoStringCount(0)]
	public record IndustryObjectProductionRateRange(
		[property: LocoStructOffset(0x00)] uint16_t min,
		[property: LocoStructOffset(0x02)] uint16_t max
		) : ILocoStruct
	{
		public static int StructSize => 0x04;
	}

	[TypeConverter(typeof(ExpandableObjectConverter))]
	[LocoStructSize(0xF4)]
	[LocoStringCount(8)]
	public record IndustryObject(
		[property: LocoStructOffset(0x00)] string_id Name,
		[property: LocoStructOffset(0x02)] string_id var_02,
		[property: LocoStructOffset(0x04)] string_id NameClosingDown,
		[property: LocoStructOffset(0x06)] string_id NameUpProduction,
		[property: LocoStructOffset(0x08)] string_id NameDownProduction,
		[property: LocoStructOffset(0x0A)] string_id NameSingular,
		[property: LocoStructOffset(0x0C)] string_id NamePlural,
		[property: LocoStructOffset(0x0E)] uint32_t var_0E, // shadows image id base
		[property: LocoStructOffset(0x12)] uint32_t var_12, // Base image id for building 0
		[property: LocoStructOffset(0x16)] uint32_t var_16,
		[property: LocoStructOffset(0x1A)] uint32_t var_1A,
		[property: LocoStructOffset(0x1E)] uint8_t var_1E,
		[property: LocoStructOffset(0x1F)] uint8_t var_1F,
		[property: LocoStructOffset(0x20), LocoArrayLength(4)] uint8_t[] BuildingPartHeight,    // This is the height of a building image
		[property: LocoStructOffset(0x24), LocoArrayLength(2)] BuildingPartAnimation[] BuildingPartAnimations, // This is 
		[property: LocoStructOffset(0x28), LocoArrayLength(4)] uint32_t[] AnimationSequences, // Access with getAnimationSequence helper method (32 bit ptr)
		[property: LocoStructOffset(0x38)] uint32_t var_38,    // Access with getUnk38 helper method (32 bit ptr)
		[property: LocoStructOffset(0x3C), LocoArrayLength(32)] uint32_t[] BuildingParts,     // Access with getBuildingParts helper method (32 bit ptr)
		[property: LocoStructOffset(0xBC)] uint8_t MinNumBuildings,
		[property: LocoStructOffset(0xBD)] uint8_t MaxNumBuildings,
		[property: LocoStructOffset(0xBE)] uint32_t Buildings, // 32 bit ptr
		[property: LocoStructOffset(0xC2)] uint32_t AvailableColours,  // bitset
		[property: LocoStructOffset(0xC6)] uint32_t BuildingSizeFlags, // flags indicating the building types size 1:large4x4, 0:small1x1
		[property: LocoStructOffset(0xCA)] uint16_t DesignedYear,
		[property: LocoStructOffset(0xCC)] uint16_t ObsoleteYear,
		[property: LocoStructOffset(0xCE)] uint8_t TotalOfTypeInScenario, // Total industries of this type that can be created in a scenario Note: this is not directly comparabile to total industries and varies based on scenario total industries cap settings. At low industries cap this value is ~3x the amount of industries in a scenario.
		[property: LocoStructOffset(0xCF)] uint8_t CostIndex,
		[property: LocoStructOffset(0xD0)] int16_t CostFactor,
		[property: LocoStructOffset(0xD2)] int16_t ClearCostFactor,
		[property: LocoStructOffset(0xD4)] uint8_t ScaffoldingSegmentType,
		[property: LocoStructOffset(0xD5)] Colour ScaffoldingColour,
		[property: LocoStructOffset(0xD6), LocoArrayLength(2)] IndustryObjectProductionRateRange[] InitialProductionRate,
		[property: LocoStructOffset(0xDE), LocoArrayLength(2)] uint8_t[] ProducedCargoType,                               // (0xFF = null)
		[property: LocoStructOffset(0xE0), LocoArrayLength(3)] uint8_t[] RequiredCargoType,                               // (0xFF = null)
		[property: LocoStructOffset(0xE3)] uint8_t pad_E3,
		[property: LocoStructOffset(0xE4)] IndustryObjectFlags Flags,
		[property: LocoStructOffset(0xE8)] uint8_t var_E8,
		[property: LocoStructOffset(0xE9)] uint8_t var_E9,
		[property: LocoStructOffset(0xEA)] uint8_t var_EA,
		[property: LocoStructOffset(0xEB)] uint8_t var_EB,
		[property: LocoStructOffset(0xEC)] uint8_t var_EC, // Used by Livestock cow shed count??
		[property: LocoStructOffset(0xED), LocoArrayLength(4)] uint8_t[] WallTypes, // There can be up to 4 different wall types for an industry
		[property: LocoStructOffset(0xF1)] uint8_t BuildingWall, // Selection of wall types isn't completely random from the 4 it is biased into 2 groups of 2 (wall and entrance)
		[property: LocoStructOffset(0xF2)] uint8_t BuildingWallEntrance, // An alternative wall type that looks like a gate placed at random places in building perimeter
		[property: LocoStructOffset(0xF3)] uint8_t var_F3
		) : ILocoStruct, ILocoStructVariableData
	{
		public static ObjectType ObjectType => ObjectType.Industry;
		public static int StructSize => 0xF4;

		public static int AnimationSequencesSize = 4;

		byte[] buildingHeight;
		BuildingPartAnimation[] animationData;

		public ReadOnlySpan<byte> Load(ReadOnlySpan<byte> remainingData)
		{
			// part heights
			buildingHeight = remainingData.Slice(0, var_1E).ToArray();
			remainingData = remainingData[(var_1E * 1)..]; // sizeof(uint8_t)

			// part animations
			animationData = remainingData.Slice(0, var_1E * BuildingPartAnimation.StructSize)
				.ToArray()
				.Chunk(BuildingPartAnimation.StructSize)
				.Select(chunk => new BuildingPartAnimation(chunk[0], chunk[1]))
				.ToArray();
			remainingData = remainingData[(var_1E * BuildingPartAnimation.StructSize)..]; // sizeof(uint8_t)

			// animation sequences
			for (var i = 0; i < AnimationSequencesSize; ++i)
			{
				var size = (remainingData[0] * 1) + 1;
				remainingData = remainingData[size..];
			}

			// unk animation related
			var ptr_38 = 0;
			while (remainingData[ptr_38] != 0xFF)
			{
				ptr_38 += IndustryObjectUnk38.StructSize;
			}
			ptr_38++;
			remainingData = remainingData[ptr_38..];

			// parts
			for (var i = 0; i < var_1F; ++i)
			{
				var ptr_1F = 0;
				while (remainingData[ptr_1F] != 0xFF)
				{
					ptr_1F++;
				}
				ptr_1F++;
				remainingData = remainingData[ptr_1F..];
			}

			// unk
			remainingData = remainingData[(MaxNumBuildings * 1)..]; // sizeof(uint8_t)

			// produced cargo
			remainingData = remainingData[(S5Header.StructLength * ProducedCargoType.Length)..];

			// required cargo
			remainingData = remainingData[(S5Header.StructLength * RequiredCargoType.Length)..];

			// wall types
			remainingData = remainingData[(S5Header.StructLength * WallTypes.Length)..];

			// unk wall type
			remainingData = remainingData[(S5Header.StructLength * 1)..];

			// unk wall type (building entrance?)
			remainingData = remainingData[(S5Header.StructLength * 1)..];

			return remainingData;
		}

		public ReadOnlySpan<byte> Save() => throw new NotImplementedException();
	}
}
