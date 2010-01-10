// SimcraftCLI.h

#pragma once

#include <msclr\marshal_cppstd.h>

using namespace System;
using namespace msclr::interop;

namespace SimcraftCLI {

	public ref class Simcraft
	{
	public:
		String^ GetItem(String^ xml)
		{  
			std::string xmlStr = marshal_as<std::string>(xml);
			xml_node_t* item_xml = xml_t::create(xmlStr);

			item_t item;
			item.sim = &sim_t();

			armory_t::parse_item_name( item, item_xml );
			armory_t::parse_item_stats( item, item_xml ) ;
			armory_t::parse_item_gems( item, item_xml ) ;
			armory_t::parse_item_weapon( item, item_xml ) ;

			item.init();

			String^ result = msclr::interop::marshal_as<String^>(item.options_str);

			return result;
		}

		double RunSim(array<String^>^ args)
		{
			sim_t sim;
			sim.seed = (int) time( NULL );
		    sim.report_progress = 0;

			int argc = args->Length + 1;
  			char** argv = new char*[ argc ];

			marshal_context ctx;
			argv[0] = "simc";
			int i = 1;
			for each (String^ arg in args)
			{
				argv[i] = (char*) ctx.marshal_as<const char*>(arg);
				i++;
			}

			double dps = 0;

			if (sim.parse_options(argc, argv))
			{
				bool success = sim.execute();

				if (success)
				{
					dps = sim.active_player->dps;
				}
			}
			
			delete argv;
			return dps;
		}
	};
}
