using Terraria.ModLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using MonoMod.RuntimeDetour.HookGen;
using Terraria;
using Terraria.ID;
using CalamityMod.NPCs.SupremeCalamitas;
using CalamityMod.NPCs.Yharon;

namespace AntiAntiButcher
{
	
	public class AntiAntiButcher : Mod
	{
		private Mod calamity => ModLoader.GetMod("CalamityMod");
		private static MethodInfo AntiButcher = null;
		
		public override void Load()
		{
			if (calamity != null)
			{
				Type calamityUtil = null;
				
				Assembly calamityAssembly = calamity.GetType().Assembly;
				foreach(Type t in calamityAssembly.GetTypes())
				{
					if (t.Name == "CalamityUtils")
					{
						//this.Logger.Info("FOUND METHOD");
						calamityUtil = t;
						break;
					}
				}
				
				if (calamityUtil != null)
				{
					AntiButcher = calamityUtil.GetMethod("AntiButcher", BindingFlags.Static | BindingFlags.Public);
					
					if (AntiButcher != null)
					{
						//this.Logger.Info("APPLYING PATCH");
						AntiAntiButcher_the_other_one += ILWeakReferences_AntiAntiButcher;
					}
				}
			}
		}
		
		public override void Unload()
		{
			
			if (AntiButcher != null) AntiAntiButcher_the_other_one -= ILWeakReferences_AntiAntiButcher;
		}
		
		private void ILWeakReferences_AntiAntiButcher(ILContext il)
		{
			var cur = new ILCursor(il);
			var label = cur.DefineLabel();
			cur.Emit(OpCodes.Ldc_I4_0);
			cur.Emit(OpCodes.Ret);
			cur.MarkLabel(label);
		}
		
		private static event ILContext.Manipulator AntiAntiButcher_the_other_one
		{
			add
			{
				HookEndpointManager.Modify(AntiButcher, value);
			}
			remove
			{
				HookEndpointManager.Unmodify(AntiButcher, value);
			}
		}
	}
	
	

	public class AntiButcherNPC : GlobalNPC
	{
		//public override bool InstancePerEntity => true;
		public override bool PreAI(NPC npc)
		{
			if (npc.modNPC is SupremeCalamitas)
			{
				SupremeCalamitas clam = (SupremeCalamitas)npc.modNPC;
				typeof(SupremeCalamitas).GetField("lootTimer", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(clam, 9001);
				//Main.NewText("poggers");
			}
			return true;
		}
		public override void PostAI(NPC npc)
		{
			if (npc.modNPC is Yharon)
			{
				Yharon chimken = (Yharon)npc.modNPC;
				bool startSecondAI = (bool)typeof(Yharon).GetField("startSecondAI", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(chimken);
				if (startSecondAI) typeof(Yharon).GetField("dropLoot", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(chimken, true);
			}
		}
		public override bool StrikeNPC(NPC npc, ref double damage, int defense, ref float knockback, int hitDirection, ref bool crit)
		{
			if (npc.modNPC is Yharon)
			{
				Yharon chimken = (Yharon)npc.modNPC;
				bool startSecondAI = (bool)typeof(Yharon).GetField("startSecondAI", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(chimken);
				if (!startSecondAI)
				{
					if (npc.life - damage <= 0)
					{
						damage = 0.0;
						npc.life = 10;
						return false;
					}
				}
				typeof(Yharon).GetField("dropLoot", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(chimken, true);
			}
			return true;
		}
	}
}