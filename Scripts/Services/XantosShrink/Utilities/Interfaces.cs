using System;
using Server;
using Server.Mobiles;
using Xanthos.Interfaces;

namespace Xanthos.Interfaces
{
	//
	// This interface is implemented by clients of ShrinkTarget allowing the
	// ShrinkTarget to adjust the charges of tools without requiring they have the same base class.
	//

	public interface IShrinkTool
	{
		int ShrinkCharges { get; set; }
	}

	//
	// Used by the auction system to validate the pet referred to by a shrink item.
	//

	public interface IShrinkItem
	{
		BaseCreature ShrunkenPet{ get; }
	}
}
