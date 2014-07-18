using BExplorer.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExplorer.Shell {
	[Obsolete("Not Used", true)]
	public static class Defaults {
		public static void AddDefaultColumns(this ShellView shellView) {

			LVCOLUMN column = new LVCOLUMN();
			column.mask = LVCF.LVCF_FMT | LVCF.LVCF_TEXT | LVCF.LVCF_WIDTH | LVCF.LVCF_SUBITEM;
			column.cx = 100;
			column.iSubItem = 0;
			column.pszText = "Name";
			column.fmt = LVCFMT.LEFT;
			User32.SendMessage(shellView.LVHandle, BExplorer.Shell.Interop.MSG.LVM_INSERTCOLUMN, 0, ref column);

			shellView.Collumns.Add(column.ToCollumns(new PROPERTYKEY() { fmtid = Guid.Parse("B725F130-47EF-101A-A5F1-02608C9EEBAC"), pid = 10 }, typeof(String), false));


			LVCOLUMN column2 = new LVCOLUMN();
			column2.mask = LVCF.LVCF_FMT | LVCF.LVCF_TEXT | LVCF.LVCF_WIDTH | LVCF.LVCF_SUBITEM;
			column2.cx = 100;
			column2.iSubItem = 1;
			column2.pszText = "Type";
			column2.fmt = LVCFMT.LEFT;
			User32.SendMessage(shellView.LVHandle, BExplorer.Shell.Interop.MSG.LVM_INSERTCOLUMN, 1, ref column2);

			shellView.Collumns.Add(column2.ToCollumns(new PROPERTYKEY() { fmtid = Guid.Parse("B725F130-47EF-101A-A5F1-02608C9EEBAC"), pid = 4 }, typeof(String), false));


			LVCOLUMN column3 = new LVCOLUMN();
			column3.mask = LVCF.LVCF_FMT | LVCF.LVCF_TEXT | LVCF.LVCF_WIDTH | LVCF.LVCF_SUBITEM;
			column3.cx = 100;
			column3.iSubItem = 2;
			column3.pszText = "Size";
			column3.fmt = LVCFMT.RIGHT;
			User32.SendMessage(shellView.LVHandle, BExplorer.Shell.Interop.MSG.LVM_INSERTCOLUMN, 2, ref column3);

			shellView.Collumns.Add(column3.ToCollumns(new PROPERTYKEY() { fmtid = Guid.Parse("B725F130-47EF-101A-A5F1-02608C9EEBAC"), pid = 12 }, typeof(long), false));


			LVCOLUMN column4 = new LVCOLUMN();
			column4.mask = LVCF.LVCF_FMT | LVCF.LVCF_TEXT | LVCF.LVCF_WIDTH | LVCF.LVCF_SUBITEM;
			column4.cx = 100;
			column4.iSubItem = 3;
			column4.pszText = "Date Modified";
			column4.fmt = LVCFMT.LEFT;
			User32.SendMessage(shellView.LVHandle, BExplorer.Shell.Interop.MSG.LVM_INSERTCOLUMN, 3, ref column4);

			shellView.Collumns.Add(column4.ToCollumns(new PROPERTYKEY() { fmtid = Guid.Parse("B725F130-47EF-101A-A5F1-02608C9EEBAC"), pid = 14 }, typeof(DateTime), false));
		}

		/// <summary>
		/// Return a Dictionary containing information about all columns available to be added to the view.
		/// </summary>
		public static Dictionary<String, Tuple<String, PROPERTYKEY, Type>> AllColumnsPKeys = new Dictionary<string, Tuple<String, PROPERTYKEY, Type>>()
				{
						{"A0", new Tuple<String, PROPERTYKEY, Type>("Name", new PROPERTYKEY(){fmtid = Guid.Parse("b725f130-47ef-101a-a5f1-02608c9eebac"), pid = 10}, typeof(String))},
						{"A1", new Tuple<String, PROPERTYKEY, Type>("Size", new PROPERTYKEY(){fmtid = Guid.Parse("b725f130-47ef-101a-a5f1-02608c9eebac"), pid = 12}, typeof(long))},
						{"A2", new Tuple<String, PROPERTYKEY, Type>("Item type", new PROPERTYKEY(){fmtid = Guid.Parse("28636aa6-953d-11d2-b5d6-00c04fd918d0"), pid = 11}, typeof(String))},
						{"A3", new Tuple<String, PROPERTYKEY, Type>("Date modified", new PROPERTYKEY(){fmtid = Guid.Parse("b725f130-47ef-101a-a5f1-02608c9eebac"), pid = 14}, typeof(DateTime))},
						{"A4", new Tuple<String, PROPERTYKEY, Type>("Date created", new PROPERTYKEY(){fmtid = Guid.Parse("b725f130-47ef-101a-a5f1-02608c9eebac"), pid = 15}, typeof(DateTime))},
						{"A5", new Tuple<String, PROPERTYKEY, Type>("Date accessed", new PROPERTYKEY(){fmtid = Guid.Parse("b725f130-47ef-101a-a5f1-02608c9eebac"), pid = 16}, typeof(DateTime))},
						{"A6", new Tuple<String, PROPERTYKEY, Type>("Attributes", new PROPERTYKEY(){fmtid = Guid.Parse("b725f130-47ef-101a-a5f1-02608c9eebac"), pid = 13}, typeof(FileAttributes))},
						{"A7", new Tuple<String, PROPERTYKEY, Type>("Offline status", new PROPERTYKEY(){fmtid = Guid.Parse("6d24888f-4718-4bda-afed-ea0fb4386cd8"), pid = 100}, typeof(String))},
						{"A8", new Tuple<String, PROPERTYKEY, Type>("Offline availability", new PROPERTYKEY(){fmtid = Guid.Parse("a94688b6-7d9f-4570-a648-e3dfc0ab2b3f"), pid = 100}, typeof(String))},
						{"A9", new Tuple<String, PROPERTYKEY, Type>("Perceived type", new PROPERTYKEY(){fmtid = Guid.Parse("28636aa6-953d-11d2-b5d6-00c04fd918d0"), pid = 9}, typeof(PerceivedType))},
						{"A10", new Tuple<String, PROPERTYKEY, Type>("Owner", new PROPERTYKEY(){fmtid = Guid.Parse("9b174b34-40ff-11d2-a27e-00c04fc30871"), pid = 4}, typeof(String))},
						{"A11", new Tuple<String, PROPERTYKEY, Type>("Kind", new PROPERTYKEY(){fmtid = Guid.Parse("1e3ee840-bc2b-476c-8237-2acd1a839b22"), pid = 3}, typeof(String))},
						{"A12", new Tuple<String, PROPERTYKEY, Type>("Date taken", new PROPERTYKEY(){fmtid = Guid.Parse("14b81da1-0135-4d31-96d9-6cbfc9671a99"), pid = 36867}, typeof(DateTime))},
						{"A13", new Tuple<String, PROPERTYKEY, Type>("Contributing artists", new PROPERTYKEY(){fmtid = Guid.Parse("56a3372e-ce9c-11d2-9f0e-006097c686f6"), pid = 2}, typeof(String))},
						{"A14", new Tuple<String, PROPERTYKEY, Type>("Album", new PROPERTYKEY(){fmtid = Guid.Parse("56a3372e-ce9c-11d2-9f0e-006097c686f6"), pid = 4}, typeof(String))},
						{"A15", new Tuple<String, PROPERTYKEY, Type>("Year", new PROPERTYKEY(){fmtid = Guid.Parse("56a3372e-ce9c-11d2-9f0e-006097c686f6"), pid = 5}, typeof(Int32))},
						{"A16", new Tuple<String, PROPERTYKEY, Type>("Genre", new PROPERTYKEY(){fmtid = Guid.Parse("56a3372e-ce9c-11d2-9f0e-006097c686f6"), pid = 11}, typeof(String))},
						{"A17", new Tuple<String, PROPERTYKEY, Type>("Conductors", new PROPERTYKEY(){fmtid = Guid.Parse("56a3372e-ce9c-11d2-9f0e-006097c686f6"), pid = 36}, typeof(String))},
						{"A18", new Tuple<String, PROPERTYKEY, Type>("Tags", new PROPERTYKEY(){fmtid = Guid.Parse("f29f85e0-4ff9-1068-ab91-08002b27b3d9"), pid = 5}, typeof(String))},
						{"A19", new Tuple<String, PROPERTYKEY, Type>("Rating", new PROPERTYKEY(){fmtid = Guid.Parse("64440492-4c8b-11d1-8b70-080036b11a03"), pid = 9}, typeof(String))},
						{"A20", new Tuple<String, PROPERTYKEY, Type>("Authors", new PROPERTYKEY(){fmtid = Guid.Parse("f29f85e0-4ff9-1068-ab91-08002b27b3d9"), pid = 4}, typeof(String))},
						{"A21", new Tuple<String, PROPERTYKEY, Type>("Title", new PROPERTYKEY(){fmtid = Guid.Parse("f29f85e0-4ff9-1068-ab91-08002b27b3d9"), pid = 2}, typeof(String))},
						{"A22", new Tuple<String, PROPERTYKEY, Type>("Subject", new PROPERTYKEY(){fmtid = Guid.Parse("f29f85e0-4ff9-1068-ab91-08002b27b3d9"), pid = 3}, typeof(String))},
						{"A23", new Tuple<String, PROPERTYKEY, Type>("Categories", new PROPERTYKEY(){fmtid = Guid.Parse("d5cdd502-2e9c-101b-9397-08002b2cf9ae"), pid = 2}, typeof(String))},
						{"A24", new Tuple<String, PROPERTYKEY, Type>("Comments", new PROPERTYKEY(){fmtid = Guid.Parse("f29f85e0-4ff9-1068-ab91-08002b27b3d9"), pid = 6}, typeof(String))},
						{"A25", new Tuple<String, PROPERTYKEY, Type>("Copyright", new PROPERTYKEY(){fmtid = Guid.Parse("64440492-4c8b-11d1-8b70-080036b11a03"), pid = 11}, typeof(String))},
						{"A26", new Tuple<String, PROPERTYKEY, Type>("#", new PROPERTYKEY(){fmtid = Guid.Parse("56a3372e-ce9c-11d2-9f0e-006097c686f6"), pid = 7}, typeof(String))},
						{"A27", new Tuple<String, PROPERTYKEY, Type>("Length", new PROPERTYKEY(){fmtid = Guid.Parse("64440490-4c8b-11d1-8b70-080036b11a03"), pid = 3}, typeof(String))},
						{"A28", new Tuple<String, PROPERTYKEY, Type>("Bit rate", new PROPERTYKEY(){fmtid = Guid.Parse("64440490-4c8b-11d1-8b70-080036b11a03"), pid = 4}, typeof(String))},
						{"A29", new Tuple<String, PROPERTYKEY, Type>("Protected", new PROPERTYKEY(){fmtid = Guid.Parse("aeac19e4-89ae-4508-b9b7-bb867abee2ed"), pid = 2}, typeof(String))},
						{"A30", new Tuple<String, PROPERTYKEY, Type>("Camera model", new PROPERTYKEY(){fmtid = Guid.Parse("14b81da1-0135-4d31-96d9-6cbfc9671a99"), pid = 272}, typeof(String))},
						{"A31", new Tuple<String, PROPERTYKEY, Type>("Dimensions", new PROPERTYKEY(){fmtid = Guid.Parse("6444048f-4c8b-11d1-8b70-080036b11a03"), pid = 13}, typeof(String))},
						{"A32", new Tuple<String, PROPERTYKEY, Type>("Camera maker", new PROPERTYKEY(){fmtid = Guid.Parse("14b81da1-0135-4d31-96d9-6cbfc9671a99"), pid = 271}, typeof(String))},
						{"A33", new Tuple<String, PROPERTYKEY, Type>("Company", new PROPERTYKEY(){fmtid = Guid.Parse("d5cdd502-2e9c-101b-9397-08002b2cf9ae"), pid = 15}, typeof(String))},
						{"A34", new Tuple<String, PROPERTYKEY, Type>("File description", new PROPERTYKEY(){fmtid = Guid.Parse("0cef7d53-fa64-11d1-a203-0000f81fedee"), pid = 3}, typeof(String))},
						{"A35", new Tuple<String, PROPERTYKEY, Type>("Program name", new PROPERTYKEY(){fmtid = Guid.Parse("f29f85e0-4ff9-1068-ab91-08002b27b3d9"), pid = 18}, typeof(String))},
						{"A36", new Tuple<String, PROPERTYKEY, Type>("Duration", new PROPERTYKEY(){fmtid = Guid.Parse("293ca35a-09aa-4dd2-b180-1fe245728a52"), pid = 100}, typeof(String))},
						{"A37", new Tuple<String, PROPERTYKEY, Type>("Is online", new PROPERTYKEY(){fmtid = Guid.Parse("bfee9149-e3e2-49a7-a862-c05988145cec"), pid = 100}, typeof(String))},
						{"A38", new Tuple<String, PROPERTYKEY, Type>("Is recurring", new PROPERTYKEY(){fmtid = Guid.Parse("315b9c8d-80a9-4ef9-ae16-8e746da51d70"), pid = 100}, typeof(String))},
						{"A39", new Tuple<String, PROPERTYKEY, Type>("Location", new PROPERTYKEY(){fmtid = Guid.Parse("f6272d18-cecc-40b1-b26a-3911717aa7bd"), pid = 100}, typeof(String))},
						{"A40", new Tuple<String, PROPERTYKEY, Type>("Optional attendee addresses", new PROPERTYKEY(){fmtid = Guid.Parse("d55bae5a-3892-417a-a649-c6ac5aaaeab3"), pid = 100}, typeof(String))},
						{"A41", new Tuple<String, PROPERTYKEY, Type>("Optional attendees", new PROPERTYKEY(){fmtid = Guid.Parse("09429607-582d-437f-84c3-de93a2b24c3c"), pid = 100}, typeof(String))},
						{"A42", new Tuple<String, PROPERTYKEY, Type>("Organizer address", new PROPERTYKEY(){fmtid = Guid.Parse("744c8242-4df5-456c-ab9e-014efb9021e3"), pid = 100}, typeof(String))},
						{"A43", new Tuple<String, PROPERTYKEY, Type>("Organizer name", new PROPERTYKEY(){fmtid = Guid.Parse("aaa660f9-9865-458e-b484-01bc7fe3973e"), pid = 100}, typeof(String))},
						{"A44", new Tuple<String, PROPERTYKEY, Type>("Reminder time", new PROPERTYKEY(){fmtid = Guid.Parse("72fc5ba4-24f9-4011-9f3f-add27afad818"), pid = 100}, typeof(String))},
						{"A45", new Tuple<String, PROPERTYKEY, Type>("Required attendee addresses", new PROPERTYKEY(){fmtid = Guid.Parse("0ba7d6c3-568d-4159-ab91-781a91fb71e5"), pid = 100}, typeof(String))},
						{"A46", new Tuple<String, PROPERTYKEY, Type>("Required attendees", new PROPERTYKEY(){fmtid = Guid.Parse("b33af30b-f552-4584-936c-cb93e5cda29f"), pid = 100}, typeof(String))},
						{"A47", new Tuple<String, PROPERTYKEY, Type>("Resources", new PROPERTYKEY(){fmtid = Guid.Parse("00f58a38-c54b-4c40-8696-97235980eae1"), pid = 100}, typeof(String))},
						{"A48", new Tuple<String, PROPERTYKEY, Type>("Meeting status", new PROPERTYKEY(){fmtid = Guid.Parse("188c1f91-3c40-4132-9ec5-d8b03b72a8a2"), pid = 100}, typeof(String))},
						{"A49", new Tuple<String, PROPERTYKEY, Type>("Free/busy status", new PROPERTYKEY(){fmtid = Guid.Parse("5bf396d4-5eb2-466f-bde9-2fb3f2361d6e"), pid = 100}, typeof(String))},
						{"A50", new Tuple<String, PROPERTYKEY, Type>("Total size", new PROPERTYKEY(){fmtid = Guid.Parse("9b174b35-40ff-11d2-a27e-00c04fc30871"), pid = 3}, typeof(String))},
						{"A51", new Tuple<String, PROPERTYKEY, Type>("Account name", new PROPERTYKEY(){fmtid = Guid.Parse("e3e0584c-b788-4a5a-bb20-7f5a44c9acdd"), pid = 9}, typeof(String))},
						{"A52", new Tuple<String, PROPERTYKEY, Type>("Task status", new PROPERTYKEY(){fmtid = Guid.Parse("be1a72c6-9a1d-46b7-afe7-afaf8cef4999"), pid = 100}, typeof(String))},
						{"A53", new Tuple<String, PROPERTYKEY, Type>("Computer", new PROPERTYKEY(){fmtid = Guid.Parse("28636aa6-953d-11d2-b5d6-00c04fd918d0"), pid = 5}, typeof(String))},
						{"A54", new Tuple<String, PROPERTYKEY, Type>("Anniversary", new PROPERTYKEY(){fmtid = Guid.Parse("9ad5badb-cea7-4470-a03d-b84e51b9949e"), pid = 100}, typeof(String))},
						{"A55", new Tuple<String, PROPERTYKEY, Type>("Assistant's name", new PROPERTYKEY(){fmtid = Guid.Parse("cd102c9c-5540-4a88-a6f6-64e4981c8cd1"), pid = 100}, typeof(String))},
						{"A56", new Tuple<String, PROPERTYKEY, Type>("Assistant's phone", new PROPERTYKEY(){fmtid = Guid.Parse("9a93244d-a7ad-4ff8-9b99-45ee4cc09af6"), pid = 100}, typeof(String))},
						{"A57", new Tuple<String, PROPERTYKEY, Type>("Birthday", new PROPERTYKEY(){fmtid = Guid.Parse("176dc63c-2688-4e89-8143-a347800f25e9"), pid = 47}, typeof(String))},
						{"A58", new Tuple<String, PROPERTYKEY, Type>("Business address", new PROPERTYKEY(){fmtid = Guid.Parse("730fb6dd-cf7c-426b-a03f-bd166cc9ee24"), pid = 100}, typeof(String))},
						{"A59", new Tuple<String, PROPERTYKEY, Type>("Business city", new PROPERTYKEY(){fmtid = Guid.Parse("402b5934-ec5a-48c3-93e6-85e86a2d934e"), pid = 100}, typeof(String))},
						{"A60", new Tuple<String, PROPERTYKEY, Type>("Business country/region", new PROPERTYKEY(){fmtid = Guid.Parse("b0b87314-fcf6-4feb-8dff-a50da6af561c"), pid = 100}, typeof(String))},
						{"A61", new Tuple<String, PROPERTYKEY, Type>("Business P.O. box", new PROPERTYKEY(){fmtid = Guid.Parse("bc4e71ce-17f9-48d5-bee9-021df0ea5409"), pid = 100}, typeof(String))},
						{"A62", new Tuple<String, PROPERTYKEY, Type>("Business postal code", new PROPERTYKEY(){fmtid = Guid.Parse("e1d4a09e-d758-4cd1-b6ec-34a8b5a73f80"), pid = 100}, typeof(String))},
						{"A63", new Tuple<String, PROPERTYKEY, Type>("Business state or province", new PROPERTYKEY(){fmtid = Guid.Parse("446f787f-10c4-41cb-a6c4-4d0343551597"), pid = 100}, typeof(String))},
						{"A64", new Tuple<String, PROPERTYKEY, Type>("Business street", new PROPERTYKEY(){fmtid = Guid.Parse("ddd1460f-c0bf-4553-8ce4-10433c908fb0"), pid = 100}, typeof(String))},
						{"A65", new Tuple<String, PROPERTYKEY, Type>("Business fax", new PROPERTYKEY(){fmtid = Guid.Parse("91eff6f3-2e27-42ca-933e-7c999fbe310b"), pid = 100}, typeof(String))},
						{"A66", new Tuple<String, PROPERTYKEY, Type>("Business home page", new PROPERTYKEY(){fmtid = Guid.Parse("56310920-2491-4919-99ce-eadb06fafdb2"), pid = 100}, typeof(String))},
						{"A67", new Tuple<String, PROPERTYKEY, Type>("Business phone", new PROPERTYKEY(){fmtid = Guid.Parse("6a15e5a0-0a1e-4cd7-bb8c-d2f1b0c929bc"), pid = 100}, typeof(String))},
						{"A68", new Tuple<String, PROPERTYKEY, Type>("Callback number", new PROPERTYKEY(){fmtid = Guid.Parse("bf53d1c3-49e0-4f7f-8567-5a821d8ac542"), pid = 100}, typeof(String))},
						{"A69", new Tuple<String, PROPERTYKEY, Type>("Car phone", new PROPERTYKEY(){fmtid = Guid.Parse("8fdc6dea-b929-412b-ba90-397a257465fe"), pid = 100}, typeof(String))},
						{"A70", new Tuple<String, PROPERTYKEY, Type>("Children", new PROPERTYKEY(){fmtid = Guid.Parse("d4729704-8ef1-43ef-9024-2bd381187fd5"), pid = 100}, typeof(String))},
						{"A71", new Tuple<String, PROPERTYKEY, Type>("Company main phone", new PROPERTYKEY(){fmtid = Guid.Parse("8589e481-6040-473d-b171-7fa89c2708ed"), pid = 100}, typeof(String))},
						{"A72", new Tuple<String, PROPERTYKEY, Type>("Department", new PROPERTYKEY(){fmtid = Guid.Parse("fc9f7306-ff8f-4d49-9fb6-3ffe5c0951ec"), pid = 100}, typeof(String))},
						{"A73", new Tuple<String, PROPERTYKEY, Type>("E-mail address", new PROPERTYKEY(){fmtid = Guid.Parse("f8fa7fa3-d12b-4785-8a4e-691a94f7a3e7"), pid = 100}, typeof(String))},
						{"A74", new Tuple<String, PROPERTYKEY, Type>("E-mail2", new PROPERTYKEY(){fmtid = Guid.Parse("38965063-edc8-4268-8491-b7723172cf29"), pid = 100}, typeof(String))},
						{"A75", new Tuple<String, PROPERTYKEY, Type>("E-mail3", new PROPERTYKEY(){fmtid = Guid.Parse("644d37b4-e1b3-4bad-b099-7e7c04966aca"), pid = 100}, typeof(String))},
						{"A76", new Tuple<String, PROPERTYKEY, Type>("E-mail list", new PROPERTYKEY(){fmtid = Guid.Parse("84d8f337-981d-44b3-9615-c7596dba17e3"), pid = 100}, typeof(String))},
						{"A77", new Tuple<String, PROPERTYKEY, Type>("E-mail display name", new PROPERTYKEY(){fmtid = Guid.Parse("cc6f4f24-6083-4bd4-8754-674d0de87ab8"), pid = 100}, typeof(String))},
						{"A78", new Tuple<String, PROPERTYKEY, Type>("File as", new PROPERTYKEY(){fmtid = Guid.Parse("f1a24aa7-9ca7-40f6-89ec-97def9ffe8db"), pid = 100}, typeof(String))},
						{"A79", new Tuple<String, PROPERTYKEY, Type>("First name", new PROPERTYKEY(){fmtid = Guid.Parse("14977844-6b49-4aad-a714-a4513bf60460"), pid = 100}, typeof(String))},
						{"A80", new Tuple<String, PROPERTYKEY, Type>("Full name", new PROPERTYKEY(){fmtid = Guid.Parse("635e9051-50a5-4ba2-b9db-4ed056c77296"), pid = 100}, typeof(String))},
						{"A81", new Tuple<String, PROPERTYKEY, Type>("Gender", new PROPERTYKEY(){fmtid = Guid.Parse("3c8cee58-d4f0-4cf9-b756-4e5d24447bcd"), pid = 100}, typeof(String))},
						{"A82", new Tuple<String, PROPERTYKEY, Type>("Given name", new PROPERTYKEY(){fmtid = Guid.Parse("176dc63c-2688-4e89-8143-a347800f25e9"), pid = 70}, typeof(String))},
						{"A83", new Tuple<String, PROPERTYKEY, Type>("Hobbies", new PROPERTYKEY(){fmtid = Guid.Parse("5dc2253f-5e11-4adf-9cfe-910dd01e3e70"), pid = 100}, typeof(String))},
						{"A84", new Tuple<String, PROPERTYKEY, Type>("Home address", new PROPERTYKEY(){fmtid = Guid.Parse("98f98354-617a-46b8-8560-5b1b64bf1f89"), pid = 100}, typeof(String))},
						{"A85", new Tuple<String, PROPERTYKEY, Type>("Home city", new PROPERTYKEY(){fmtid = Guid.Parse("176dc63c-2688-4e89-8143-a347800f25e9"), pid = 65}, typeof(String))},
						{"A86", new Tuple<String, PROPERTYKEY, Type>("Home country/region", new PROPERTYKEY(){fmtid = Guid.Parse("08a65aa1-f4c9-43dd-9ddf-a33d8e7ead85"), pid = 100}, typeof(String))},
						{"A87", new Tuple<String, PROPERTYKEY, Type>("Home P.O. box", new PROPERTYKEY(){fmtid = Guid.Parse("7b9f6399-0a3f-4b12-89bd-4adc51c918af"), pid = 100}, typeof(String))},
						{"A88", new Tuple<String, PROPERTYKEY, Type>("Home postal code", new PROPERTYKEY(){fmtid = Guid.Parse("8afcc170-8a46-4b53-9eee-90bae7151e62"), pid = 100}, typeof(String))},
						{"A89", new Tuple<String, PROPERTYKEY, Type>("Home state or province", new PROPERTYKEY(){fmtid = Guid.Parse("c89a23d0-7d6d-4eb8-87d4-776a82d493e5"), pid = 100}, typeof(String))},
						{"A90", new Tuple<String, PROPERTYKEY, Type>("Home street", new PROPERTYKEY(){fmtid = Guid.Parse("0adef160-db3f-4308-9a21-06237b16fa2a"), pid = 100}, typeof(String))},
						{"A91", new Tuple<String, PROPERTYKEY, Type>("Home fax", new PROPERTYKEY(){fmtid = Guid.Parse("660e04d6-81ab-4977-a09f-82313113ab26"), pid = 100}, typeof(String))},
						{"A92", new Tuple<String, PROPERTYKEY, Type>("Home phone", new PROPERTYKEY(){fmtid = Guid.Parse("176dc63c-2688-4e89-8143-a347800f25e9"), pid = 20}, typeof(String))},
						{"A93", new Tuple<String, PROPERTYKEY, Type>("IM addresses", new PROPERTYKEY(){fmtid = Guid.Parse("d68dbd8a-3374-4b81-9972-3ec30682db3d"), pid = 100}, typeof(String))},
						{"A94", new Tuple<String, PROPERTYKEY, Type>("Initials", new PROPERTYKEY(){fmtid = Guid.Parse("f3d8f40d-50cb-44a2-9718-40cb9119495d"), pid = 100}, typeof(String))},
						{"A95", new Tuple<String, PROPERTYKEY, Type>("Job title", new PROPERTYKEY(){fmtid = Guid.Parse("176dc63c-2688-4e89-8143-a347800f25e9"), pid = 6}, typeof(String))},
						{"A96", new Tuple<String, PROPERTYKEY, Type>("Label", new PROPERTYKEY(){fmtid = Guid.Parse("97b0ad89-df49-49cc-834e-660974fd755b"), pid = 100}, typeof(String))},
						{"A97", new Tuple<String, PROPERTYKEY, Type>("Last name", new PROPERTYKEY(){fmtid = Guid.Parse("8f367200-c270-457c-b1d4-e07c5bcd90c7"), pid = 100}, typeof(String))},
						{"A98", new Tuple<String, PROPERTYKEY, Type>("Mailing address", new PROPERTYKEY(){fmtid = Guid.Parse("c0ac206a-827e-4650-95ae-77e2bb74fcc9"), pid = 100}, typeof(String))},
						{"A99", new Tuple<String, PROPERTYKEY, Type>("Middle name", new PROPERTYKEY(){fmtid = Guid.Parse("176dc63c-2688-4e89-8143-a347800f25e9"), pid = 71}, typeof(String))},
						{"A100", new Tuple<String, PROPERTYKEY, Type>("Cell phone", new PROPERTYKEY(){fmtid = Guid.Parse("176dc63c-2688-4e89-8143-a347800f25e9"), pid = 35}, typeof(String))},
						{"A101", new Tuple<String, PROPERTYKEY, Type>("Nickname", new PROPERTYKEY(){fmtid = Guid.Parse("176dc63c-2688-4e89-8143-a347800f25e9"), pid = 74}, typeof(String))},
						{"A102", new Tuple<String, PROPERTYKEY, Type>("Office location", new PROPERTYKEY(){fmtid = Guid.Parse("176dc63c-2688-4e89-8143-a347800f25e9"), pid = 7}, typeof(String))},
						{"A103", new Tuple<String, PROPERTYKEY, Type>("Other address", new PROPERTYKEY(){fmtid = Guid.Parse("508161fa-313b-43d5-83a1-c1accf68622c"), pid = 100}, typeof(String))},
						{"A104", new Tuple<String, PROPERTYKEY, Type>("Other city", new PROPERTYKEY(){fmtid = Guid.Parse("6e682923-7f7b-4f0c-a337-cfca296687bf"), pid = 100}, typeof(String))},
						{"A105", new Tuple<String, PROPERTYKEY, Type>("Other country/region", new PROPERTYKEY(){fmtid = Guid.Parse("8f167568-0aae-4322-8ed9-6055b7b0e398"), pid = 100}, typeof(String))},
						{"A106", new Tuple<String, PROPERTYKEY, Type>("Other P.O. box", new PROPERTYKEY(){fmtid = Guid.Parse("8b26ea41-058f-43f6-aecc-4035681ce977"), pid = 100}, typeof(String))},
						{"A107", new Tuple<String, PROPERTYKEY, Type>("Other postal code", new PROPERTYKEY(){fmtid = Guid.Parse("95c656c1-2abf-4148-9ed3-9ec602e3b7cd"), pid = 100}, typeof(String))},
						{"A108", new Tuple<String, PROPERTYKEY, Type>("Other state or province", new PROPERTYKEY(){fmtid = Guid.Parse("71b377d6-e570-425f-a170-809fae73e54e"), pid = 100}, typeof(String))},
						{"A109", new Tuple<String, PROPERTYKEY, Type>("Other street", new PROPERTYKEY(){fmtid = Guid.Parse("ff962609-b7d6-4999-862d-95180d529aea"), pid = 100}, typeof(String))},
						{"A110", new Tuple<String, PROPERTYKEY, Type>("Pager", new PROPERTYKEY(){fmtid = Guid.Parse("d6304e01-f8f5-4f45-8b15-d024a6296789"), pid = 100}, typeof(String))},
						{"A111", new Tuple<String, PROPERTYKEY, Type>("Personal title", new PROPERTYKEY(){fmtid = Guid.Parse("176dc63c-2688-4e89-8143-a347800f25e9"), pid = 69}, typeof(String))},
						{"A112", new Tuple<String, PROPERTYKEY, Type>("City", new PROPERTYKEY(){fmtid = Guid.Parse("c8ea94f0-a9e3-4969-a94b-9c62a95324e0"), pid = 100}, typeof(String))},
						{"A113", new Tuple<String, PROPERTYKEY, Type>("Country/region", new PROPERTYKEY(){fmtid = Guid.Parse("e53d799d-0f3f-466e-b2ff-74634a3cb7a4"), pid = 100}, typeof(String))},
						{"A114", new Tuple<String, PROPERTYKEY, Type>("P.O. box", new PROPERTYKEY(){fmtid = Guid.Parse("de5ef3c7-46e1-484e-9999-62c5308394c1"), pid = 100}, typeof(String))},
						{"A115", new Tuple<String, PROPERTYKEY, Type>("Postal code", new PROPERTYKEY(){fmtid = Guid.Parse("18bbd425-ecfd-46ef-b612-7b4a6034eda0"), pid = 100}, typeof(String))},
						{"A116", new Tuple<String, PROPERTYKEY, Type>("State or province", new PROPERTYKEY(){fmtid = Guid.Parse("f1176dfe-7138-4640-8b4c-ae375dc70a6d"), pid = 100}, typeof(String))},
						{"A117", new Tuple<String, PROPERTYKEY, Type>("Street", new PROPERTYKEY(){fmtid = Guid.Parse("63c25b20-96be-488f-8788-c09c407ad812"), pid = 100}, typeof(String))},
						{"A118", new Tuple<String, PROPERTYKEY, Type>("Primary e-mail", new PROPERTYKEY(){fmtid = Guid.Parse("176dc63c-2688-4e89-8143-a347800f25e9"), pid = 48}, typeof(String))},
						{"A119", new Tuple<String, PROPERTYKEY, Type>("Primary phone", new PROPERTYKEY(){fmtid = Guid.Parse("176dc63c-2688-4e89-8143-a347800f25e9"), pid = 25}, typeof(String))},
						{"A120", new Tuple<String, PROPERTYKEY, Type>("Profession", new PROPERTYKEY(){fmtid = Guid.Parse("7268af55-1ce4-4f6e-a41f-b6e4ef10e4a9"), pid = 100}, typeof(String))},
						{"A121", new Tuple<String, PROPERTYKEY, Type>("Spouse/Partner", new PROPERTYKEY(){fmtid = Guid.Parse("9d2408b6-3167-422b-82b0-f583b7a7cfe3"), pid = 100}, typeof(String))},
						{"A122", new Tuple<String, PROPERTYKEY, Type>("Suffix", new PROPERTYKEY(){fmtid = Guid.Parse("176dc63c-2688-4e89-8143-a347800f25e9"), pid = 73}, typeof(String))},
						{"A123", new Tuple<String, PROPERTYKEY, Type>("TTY/TTD phone", new PROPERTYKEY(){fmtid = Guid.Parse("aaf16bac-2b55-45e6-9f6d-415eb94910df"), pid = 100}, typeof(String))},
						{"A124", new Tuple<String, PROPERTYKEY, Type>("Telex", new PROPERTYKEY(){fmtid = Guid.Parse("c554493c-c1f7-40c1-a76c-ef8c0614003e"), pid = 100}, typeof(String))},
						{"A125", new Tuple<String, PROPERTYKEY, Type>("Webpage", new PROPERTYKEY(){fmtid = Guid.Parse("e3e0584c-b788-4a5a-bb20-7f5a44c9acdd"), pid = 18}, typeof(String))},
						{"A126", new Tuple<String, PROPERTYKEY, Type>("Content status", new PROPERTYKEY(){fmtid = Guid.Parse("d5cdd502-2e9c-101b-9397-08002b2cf9ae"), pid = 27}, typeof(String))},
						{"A127", new Tuple<String, PROPERTYKEY, Type>("Content type", new PROPERTYKEY(){fmtid = Guid.Parse("d5cdd502-2e9c-101b-9397-08002b2cf9ae"), pid = 26}, typeof(String))},
						{"A128", new Tuple<String, PROPERTYKEY, Type>("Date acquired", new PROPERTYKEY(){fmtid = Guid.Parse("2cbaa8f5-d81f-47ca-b17a-f8d822300131"), pid = 100}, typeof(String))},
						{"A129", new Tuple<String, PROPERTYKEY, Type>("Date archived", new PROPERTYKEY(){fmtid = Guid.Parse("43f8d7b7-a444-4f87-9383-52271c9b915c"), pid = 100}, typeof(String))},
						{"A130", new Tuple<String, PROPERTYKEY, Type>("Date completed", new PROPERTYKEY(){fmtid = Guid.Parse("72fab781-acda-43e5-b155-b2434f85e678"), pid = 100}, typeof(String))},
						{"A131", new Tuple<String, PROPERTYKEY, Type>("Device category", new PROPERTYKEY(){fmtid = Guid.Parse("78c34fc8-104a-4aca-9ea4-524d52996e57"), pid = 94}, typeof(String))},
						{"A132", new Tuple<String, PROPERTYKEY, Type>("Connected", new PROPERTYKEY(){fmtid = Guid.Parse("78c34fc8-104a-4aca-9ea4-524d52996e57"), pid = 55}, typeof(String))},
						{"A133", new Tuple<String, PROPERTYKEY, Type>("Discovery method", new PROPERTYKEY(){fmtid = Guid.Parse("78c34fc8-104a-4aca-9ea4-524d52996e57"), pid = 52}, typeof(String))},
						{"A134", new Tuple<String, PROPERTYKEY, Type>("Friendly name", new PROPERTYKEY(){fmtid = Guid.Parse("656a3bb3-ecc0-43fd-8477-4ae0404a96cd"), pid = 12288}, typeof(String))},
						{"A135", new Tuple<String, PROPERTYKEY, Type>("Local computer", new PROPERTYKEY(){fmtid = Guid.Parse("78c34fc8-104a-4aca-9ea4-524d52996e57"), pid = 70}, typeof(String))},
						{"A136", new Tuple<String, PROPERTYKEY, Type>("Manufacturer", new PROPERTYKEY(){fmtid = Guid.Parse("656a3bb3-ecc0-43fd-8477-4ae0404a96cd"), pid = 8192}, typeof(String))},
						{"A137", new Tuple<String, PROPERTYKEY, Type>("Model", new PROPERTYKEY(){fmtid = Guid.Parse("656a3bb3-ecc0-43fd-8477-4ae0404a96cd"), pid = 8194}, typeof(String))},
						{"A138", new Tuple<String, PROPERTYKEY, Type>("Paired", new PROPERTYKEY(){fmtid = Guid.Parse("78c34fc8-104a-4aca-9ea4-524d52996e57"), pid = 56}, typeof(String))},
						{"A139", new Tuple<String, PROPERTYKEY, Type>("Classification", new PROPERTYKEY(){fmtid = Guid.Parse("d08dd4c0-3a9e-462e-8290-7b636b2576b9"), pid = 10}, typeof(String))},
						{"A140", new Tuple<String, PROPERTYKEY, Type>("Status", new PROPERTYKEY(){fmtid = Guid.Parse("d08dd4c0-3a9e-462e-8290-7b636b2576b9"), pid = 257}, typeof(String))},
						{"A141", new Tuple<String, PROPERTYKEY, Type>("Client ID", new PROPERTYKEY(){fmtid = Guid.Parse("276d7bb0-5b34-4fb0-aa4b-158ed12a1809"), pid = 100}, typeof(String))},
						{"A142", new Tuple<String, PROPERTYKEY, Type>("Contributors", new PROPERTYKEY(){fmtid = Guid.Parse("f334115e-da1b-4509-9b3d-119504dc7abb"), pid = 100}, typeof(String))},
						{"A143", new Tuple<String, PROPERTYKEY, Type>("Content created", new PROPERTYKEY(){fmtid = Guid.Parse("f29f85e0-4ff9-1068-ab91-08002b27b3d9"), pid = 12}, typeof(String))},
						{"A144", new Tuple<String, PROPERTYKEY, Type>("Last printed", new PROPERTYKEY(){fmtid = Guid.Parse("f29f85e0-4ff9-1068-ab91-08002b27b3d9"), pid = 11}, typeof(String))},
						{"A145", new Tuple<String, PROPERTYKEY, Type>("Date last saved", new PROPERTYKEY(){fmtid = Guid.Parse("f29f85e0-4ff9-1068-ab91-08002b27b3d9"), pid = 13}, typeof(String))},
						{"A146", new Tuple<String, PROPERTYKEY, Type>("Division", new PROPERTYKEY(){fmtid = Guid.Parse("1e005ee6-bf27-428b-b01c-79676acd2870"), pid = 100}, typeof(String))},
						{"A147", new Tuple<String, PROPERTYKEY, Type>("Document ID", new PROPERTYKEY(){fmtid = Guid.Parse("e08805c8-e395-40df-80d2-54f0d6c43154"), pid = 100}, typeof(String))},
						{"A148", new Tuple<String, PROPERTYKEY, Type>("Pages", new PROPERTYKEY(){fmtid = Guid.Parse("f29f85e0-4ff9-1068-ab91-08002b27b3d9"), pid = 14}, typeof(String))},
						{"A149", new Tuple<String, PROPERTYKEY, Type>("Slides", new PROPERTYKEY(){fmtid = Guid.Parse("d5cdd502-2e9c-101b-9397-08002b2cf9ae"), pid = 7}, typeof(String))},
						{"A150", new Tuple<String, PROPERTYKEY, Type>("Total editing time", new PROPERTYKEY(){fmtid = Guid.Parse("f29f85e0-4ff9-1068-ab91-08002b27b3d9"), pid = 10}, typeof(String))},
						{"A151", new Tuple<String, PROPERTYKEY, Type>("Word count", new PROPERTYKEY(){fmtid = Guid.Parse("f29f85e0-4ff9-1068-ab91-08002b27b3d9"), pid = 15}, typeof(String))},
						{"A152", new Tuple<String, PROPERTYKEY, Type>("Due date", new PROPERTYKEY(){fmtid = Guid.Parse("3f8472b5-e0af-4db2-8071-c53fe76ae7ce"), pid = 100}, typeof(String))},
						{"A153", new Tuple<String, PROPERTYKEY, Type>("End date", new PROPERTYKEY(){fmtid = Guid.Parse("c75faa05-96fd-49e7-9cb4-9f601082d553"), pid = 100}, typeof(String))},
						{"A154", new Tuple<String, PROPERTYKEY, Type>("File count", new PROPERTYKEY(){fmtid = Guid.Parse("28636aa6-953d-11d2-b5d6-00c04fd918d0"), pid = 12}, typeof(String))},
						{"A155", new Tuple<String, PROPERTYKEY, Type>("Filename", new PROPERTYKEY(){fmtid = Guid.Parse("41cf5ae0-f75a-4806-bd87-59c7d9248eb9"), pid = 100}, typeof(String))},
						{"A156", new Tuple<String, PROPERTYKEY, Type>("File version", new PROPERTYKEY(){fmtid = Guid.Parse("0cef7d53-fa64-11d1-a203-0000f81fedee"), pid = 4}, typeof(String))},
						{"A157", new Tuple<String, PROPERTYKEY, Type>("Flag color", new PROPERTYKEY(){fmtid = Guid.Parse("67df94de-0ca7-4d6f-b792-053a3e4f03cf"), pid = 100}, typeof(String))},
						{"A158", new Tuple<String, PROPERTYKEY, Type>("Flag status", new PROPERTYKEY(){fmtid = Guid.Parse("e3e0584c-b788-4a5a-bb20-7f5a44c9acdd"), pid = 12}, typeof(String))},
						{"A159", new Tuple<String, PROPERTYKEY, Type>("Space free", new PROPERTYKEY(){fmtid = Guid.Parse("9b174b35-40ff-11d2-a27e-00c04fc30871"), pid = 2}, typeof(String))},
						{"A160", new Tuple<String, PROPERTYKEY, Type>("Bit depth", new PROPERTYKEY(){fmtid = Guid.Parse("6444048f-4c8b-11d1-8b70-080036b11a03"), pid = 7}, typeof(String))},
						{"A161", new Tuple<String, PROPERTYKEY, Type>("Horizontal resolution", new PROPERTYKEY(){fmtid = Guid.Parse("6444048f-4c8b-11d1-8b70-080036b11a03"), pid = 5}, typeof(String))},
						{"A162", new Tuple<String, PROPERTYKEY, Type>("Width", new PROPERTYKEY(){fmtid = Guid.Parse("6444048f-4c8b-11d1-8b70-080036b11a03"), pid = 3}, typeof(String))},
						{"A163", new Tuple<String, PROPERTYKEY, Type>("Vertical resolution", new PROPERTYKEY(){fmtid = Guid.Parse("6444048f-4c8b-11d1-8b70-080036b11a03"), pid = 6}, typeof(String))},
						{"A164", new Tuple<String, PROPERTYKEY, Type>("Height", new PROPERTYKEY(){fmtid = Guid.Parse("6444048f-4c8b-11d1-8b70-080036b11a03"), pid = 4}, typeof(String))},
						{"A165", new Tuple<String, PROPERTYKEY, Type>("Importance", new PROPERTYKEY(){fmtid = Guid.Parse("e3e0584c-b788-4a5a-bb20-7f5a44c9acdd"), pid = 11}, typeof(String))},
						{"A166", new Tuple<String, PROPERTYKEY, Type>("Is attachment", new PROPERTYKEY(){fmtid = Guid.Parse("f23f425c-71a1-4fa8-922f-678ea4a60408"), pid = 100}, typeof(String))},
						{"A167", new Tuple<String, PROPERTYKEY, Type>("Is deleted", new PROPERTYKEY(){fmtid = Guid.Parse("5cda5fc8-33ee-4ff3-9094-ae7bd8868c4d"), pid = 100}, typeof(String))},
						{"A168", new Tuple<String, PROPERTYKEY, Type>("Encryption status", new PROPERTYKEY(){fmtid = Guid.Parse("90e5e14e-648b-4826-b2aa-acaf790e3513"), pid = 10}, typeof(String))},
						{"A169", new Tuple<String, PROPERTYKEY, Type>("Has flag", new PROPERTYKEY(){fmtid = Guid.Parse("5da84765-e3ff-4278-86b0-a27967fbdd03"), pid = 100}, typeof(String))},
						{"A170", new Tuple<String, PROPERTYKEY, Type>("Is completed", new PROPERTYKEY(){fmtid = Guid.Parse("a6f360d2-55f9-48de-b909-620e090a647c"), pid = 100}, typeof(String))},
						{"A171", new Tuple<String, PROPERTYKEY, Type>("Incomplete", new PROPERTYKEY(){fmtid = Guid.Parse("346c8bd1-2e6a-4c45-89a4-61b78e8e700f"), pid = 100}, typeof(String))},
						{"A172", new Tuple<String, PROPERTYKEY, Type>("Read status", new PROPERTYKEY(){fmtid = Guid.Parse("e3e0584c-b788-4a5a-bb20-7f5a44c9acdd"), pid = 10}, typeof(String))},
						{"A173", new Tuple<String, PROPERTYKEY, Type>("Shared", new PROPERTYKEY(){fmtid = Guid.Parse("ef884c5b-2bfe-41bb-aae5-76eedf4f9902"), pid = 100}, typeof(String))},
						{"A174", new Tuple<String, PROPERTYKEY, Type>("Creators", new PROPERTYKEY(){fmtid = Guid.Parse("d0a04f0a-462a-48a4-bb2f-3706e88dbd7d"), pid = 100}, typeof(String))},
						{"A175", new Tuple<String, PROPERTYKEY, Type>("Date", new PROPERTYKEY(){fmtid = Guid.Parse("f7db74b4-4287-4103-afba-f1b13dcd75cf"), pid = 100}, typeof(String))},
						{"A176", new Tuple<String, PROPERTYKEY, Type>("Folder name", new PROPERTYKEY(){fmtid = Guid.Parse("b725f130-47ef-101a-a5f1-02608c9eebac"), pid = 2}, typeof(String))},
						{"A177", new Tuple<String, PROPERTYKEY, Type>("Folder path", new PROPERTYKEY(){fmtid = Guid.Parse("e3e0584c-b788-4a5a-bb20-7f5a44c9acdd"), pid = 6}, typeof(String))},
						{"A178", new Tuple<String, PROPERTYKEY, Type>("Folder", new PROPERTYKEY(){fmtid = Guid.Parse("dabd30ed-0043-4789-a7f8-d013a4736622"), pid = 100}, typeof(String))},
						{"A179", new Tuple<String, PROPERTYKEY, Type>("Participants", new PROPERTYKEY(){fmtid = Guid.Parse("d4d0aa16-9948-41a4-aa85-d97ff9646993"), pid = 100}, typeof(String))},
						{"A180", new Tuple<String, PROPERTYKEY, Type>("Path", new PROPERTYKEY(){fmtid = Guid.Parse("e3e0584c-b788-4a5a-bb20-7f5a44c9acdd"), pid = 7}, typeof(String))},
						{"A181", new Tuple<String, PROPERTYKEY, Type>("By location", new PROPERTYKEY(){fmtid = Guid.Parse("23620678-ccd4-47c0-9963-95a8405678a3"), pid = 100}, typeof(String))},
						{"A182", new Tuple<String, PROPERTYKEY, Type>("Type", new PROPERTYKEY(){fmtid = Guid.Parse("b725f130-47ef-101a-a5f1-02608c9eebac"), pid = 4}, typeof(String))},
						{"A183", new Tuple<String, PROPERTYKEY, Type>("Contact names", new PROPERTYKEY(){fmtid = Guid.Parse("dea7c82c-1d89-4a66-9427-a4e3debabcb1"), pid = 100}, typeof(String))},
						{"A184", new Tuple<String, PROPERTYKEY, Type>("Entry type", new PROPERTYKEY(){fmtid = Guid.Parse("95beb1fc-326d-4644-b396-cd3ed90e6ddf"), pid = 100}, typeof(String))},
						{"A185", new Tuple<String, PROPERTYKEY, Type>("Language", new PROPERTYKEY(){fmtid = Guid.Parse("d5cdd502-2e9c-101b-9397-08002b2cf9ae"), pid = 28}, typeof(String))},
						{"A186", new Tuple<String, PROPERTYKEY, Type>("Date visited", new PROPERTYKEY(){fmtid = Guid.Parse("5cbf2787-48cf-4208-b90e-ee5e5d420294"), pid = 23}, typeof(String))},
						{"A187", new Tuple<String, PROPERTYKEY, Type>("Description", new PROPERTYKEY(){fmtid = Guid.Parse("5cbf2787-48cf-4208-b90e-ee5e5d420294"), pid = 21}, typeof(String))},
						{"A188", new Tuple<String, PROPERTYKEY, Type>("Link status", new PROPERTYKEY(){fmtid = Guid.Parse("b9b4b3fc-2b51-4a42-b5d8-324146afcf25"), pid = 3}, typeof(String))},
						{"A189", new Tuple<String, PROPERTYKEY, Type>("Link target", new PROPERTYKEY(){fmtid = Guid.Parse("b9b4b3fc-2b51-4a42-b5d8-324146afcf25"), pid = 2}, typeof(String))},
						{"A190", new Tuple<String, PROPERTYKEY, Type>("URL", new PROPERTYKEY(){fmtid = Guid.Parse("5cbf2787-48cf-4208-b90e-ee5e5d420294"), pid = 2}, typeof(String))},
						{"A191", new Tuple<String, PROPERTYKEY, Type>("Media created", new PROPERTYKEY(){fmtid = Guid.Parse("2e4b640d-5019-46d8-8881-55414cc5caa0"), pid = 100}, typeof(String))},
						{"A192", new Tuple<String, PROPERTYKEY, Type>("Date released", new PROPERTYKEY(){fmtid = Guid.Parse("de41cc29-6971-4290-b472-f59f2e2f31e2"), pid = 100}, typeof(String))},
						{"A193", new Tuple<String, PROPERTYKEY, Type>("Encoded by", new PROPERTYKEY(){fmtid = Guid.Parse("64440492-4c8b-11d1-8b70-080036b11a03"), pid = 36}, typeof(String))},
						{"A194", new Tuple<String, PROPERTYKEY, Type>("Producers", new PROPERTYKEY(){fmtid = Guid.Parse("64440492-4c8b-11d1-8b70-080036b11a03"), pid = 22}, typeof(String))},
						{"A195", new Tuple<String, PROPERTYKEY, Type>("Publisher", new PROPERTYKEY(){fmtid = Guid.Parse("64440492-4c8b-11d1-8b70-080036b11a03"), pid = 30}, typeof(String))},
						{"A196", new Tuple<String, PROPERTYKEY, Type>("Subtitle", new PROPERTYKEY(){fmtid = Guid.Parse("56a3372e-ce9c-11d2-9f0e-006097c686f6"), pid = 38}, typeof(String))},
						{"A197", new Tuple<String, PROPERTYKEY, Type>("User web URL", new PROPERTYKEY(){fmtid = Guid.Parse("64440492-4c8b-11d1-8b70-080036b11a03"), pid = 34}, typeof(String))},
						{"A198", new Tuple<String, PROPERTYKEY, Type>("Writers", new PROPERTYKEY(){fmtid = Guid.Parse("64440492-4c8b-11d1-8b70-080036b11a03"), pid = 23}, typeof(String))},
						{"A199", new Tuple<String, PROPERTYKEY, Type>("Attachments", new PROPERTYKEY(){fmtid = Guid.Parse("e3e0584c-b788-4a5a-bb20-7f5a44c9acdd"), pid = 21}, typeof(String))},
						{"A200", new Tuple<String, PROPERTYKEY, Type>("Bcc addresses", new PROPERTYKEY(){fmtid = Guid.Parse("e3e0584c-b788-4a5a-bb20-7f5a44c9acdd"), pid = 2}, typeof(String))},
						{"A201", new Tuple<String, PROPERTYKEY, Type>("Bcc", new PROPERTYKEY(){fmtid = Guid.Parse("e3e0584c-b788-4a5a-bb20-7f5a44c9acdd"), pid = 3}, typeof(String))},
						{"A202", new Tuple<String, PROPERTYKEY, Type>("Cc addresses", new PROPERTYKEY(){fmtid = Guid.Parse("e3e0584c-b788-4a5a-bb20-7f5a44c9acdd"), pid = 4}, typeof(String))},
						{"A203", new Tuple<String, PROPERTYKEY, Type>("Cc", new PROPERTYKEY(){fmtid = Guid.Parse("e3e0584c-b788-4a5a-bb20-7f5a44c9acdd"), pid = 5}, typeof(String))},
						{"A204", new Tuple<String, PROPERTYKEY, Type>("Conversation ID", new PROPERTYKEY(){fmtid = Guid.Parse("dc8f80bd-af1e-4289-85b6-3dfc1b493992"), pid = 100}, typeof(String))},
						{"A205", new Tuple<String, PROPERTYKEY, Type>("Date received", new PROPERTYKEY(){fmtid = Guid.Parse("e3e0584c-b788-4a5a-bb20-7f5a44c9acdd"), pid = 20}, typeof(String))},
						{"A206", new Tuple<String, PROPERTYKEY, Type>("Date sent", new PROPERTYKEY(){fmtid = Guid.Parse("e3e0584c-b788-4a5a-bb20-7f5a44c9acdd"), pid = 19}, typeof(String))},
						{"A207", new Tuple<String, PROPERTYKEY, Type>("From addresses", new PROPERTYKEY(){fmtid = Guid.Parse("e3e0584c-b788-4a5a-bb20-7f5a44c9acdd"), pid = 13}, typeof(String))},
						{"A208", new Tuple<String, PROPERTYKEY, Type>("From", new PROPERTYKEY(){fmtid = Guid.Parse("e3e0584c-b788-4a5a-bb20-7f5a44c9acdd"), pid = 14}, typeof(String))},
						{"A209", new Tuple<String, PROPERTYKEY, Type>("Has attachments", new PROPERTYKEY(){fmtid = Guid.Parse("9c1fcf74-2d97-41ba-b4ae-cb2e3661a6e4"), pid = 8}, typeof(String))},
						{"A210", new Tuple<String, PROPERTYKEY, Type>("Sender address", new PROPERTYKEY(){fmtid = Guid.Parse("0be1c8e7-1981-4676-ae14-fdd78f05a6e7"), pid = 100}, typeof(String))},
						{"A211", new Tuple<String, PROPERTYKEY, Type>("Sender name", new PROPERTYKEY(){fmtid = Guid.Parse("0da41cfa-d224-4a18-ae2f-596158db4b3a"), pid = 100}, typeof(String))},
						{"A212", new Tuple<String, PROPERTYKEY, Type>("Store", new PROPERTYKEY(){fmtid = Guid.Parse("e3e0584c-b788-4a5a-bb20-7f5a44c9acdd"), pid = 15}, typeof(String))},
						{"A213", new Tuple<String, PROPERTYKEY, Type>("To addresses", new PROPERTYKEY(){fmtid = Guid.Parse("e3e0584c-b788-4a5a-bb20-7f5a44c9acdd"), pid = 16}, typeof(String))},
						{"A214", new Tuple<String, PROPERTYKEY, Type>("To do title", new PROPERTYKEY(){fmtid = Guid.Parse("bccc8a3c-8cef-42e5-9b1c-c69079398bc7"), pid = 100}, typeof(String))},
						{"A215", new Tuple<String, PROPERTYKEY, Type>("To", new PROPERTYKEY(){fmtid = Guid.Parse("e3e0584c-b788-4a5a-bb20-7f5a44c9acdd"), pid = 17}, typeof(String))},
						{"A216", new Tuple<String, PROPERTYKEY, Type>("Mileage", new PROPERTYKEY(){fmtid = Guid.Parse("fdf84370-031a-4add-9e91-0d775f1c6605"), pid = 100}, typeof(String))},
						{"A217", new Tuple<String, PROPERTYKEY, Type>("Album artist", new PROPERTYKEY(){fmtid = Guid.Parse("56a3372e-ce9c-11d2-9f0e-006097c686f6"), pid = 13}, typeof(String))},
						{"A218", new Tuple<String, PROPERTYKEY, Type>("Album ID", new PROPERTYKEY(){fmtid = Guid.Parse("56a3372e-ce9c-11d2-9f0e-006097c686f6"), pid = 100}, typeof(String))},
						{"A219", new Tuple<String, PROPERTYKEY, Type>("Beats-per-minute", new PROPERTYKEY(){fmtid = Guid.Parse("56a3372e-ce9c-11d2-9f0e-006097c686f6"), pid = 35}, typeof(String))},
						{"A220", new Tuple<String, PROPERTYKEY, Type>("Composers", new PROPERTYKEY(){fmtid = Guid.Parse("64440492-4c8b-11d1-8b70-080036b11a03"), pid = 19}, typeof(String))},
						{"A221", new Tuple<String, PROPERTYKEY, Type>("Initial key", new PROPERTYKEY(){fmtid = Guid.Parse("56a3372e-ce9c-11d2-9f0e-006097c686f6"), pid = 34}, typeof(String))},
						{"A222", new Tuple<String, PROPERTYKEY, Type>("Part of a compilation", new PROPERTYKEY(){fmtid = Guid.Parse("c449d5cb-9ea4-4809-82e8-af9d59ded6d1"), pid = 100}, typeof(String))},
						{"A223", new Tuple<String, PROPERTYKEY, Type>("Mood", new PROPERTYKEY(){fmtid = Guid.Parse("56a3372e-ce9c-11d2-9f0e-006097c686f6"), pid = 39}, typeof(String))},
						{"A224", new Tuple<String, PROPERTYKEY, Type>("Part of set", new PROPERTYKEY(){fmtid = Guid.Parse("56a3372e-ce9c-11d2-9f0e-006097c686f6"), pid = 37}, typeof(String))},
						{"A225", new Tuple<String, PROPERTYKEY, Type>("Period", new PROPERTYKEY(){fmtid = Guid.Parse("64440492-4c8b-11d1-8b70-080036b11a03"), pid = 31}, typeof(String))},
						{"A226", new Tuple<String, PROPERTYKEY, Type>("Color", new PROPERTYKEY(){fmtid = Guid.Parse("4776cafa-bce4-4cb1-a23e-265e76d8eb11"), pid = 100}, typeof(String))},
						{"A227", new Tuple<String, PROPERTYKEY, Type>("Parental rating", new PROPERTYKEY(){fmtid = Guid.Parse("64440492-4c8b-11d1-8b70-080036b11a03"), pid = 21}, typeof(String))},
						{"A228", new Tuple<String, PROPERTYKEY, Type>("Parental rating reason", new PROPERTYKEY(){fmtid = Guid.Parse("10984e0a-f9f2-4321-b7ef-baf195af4319"), pid = 100}, typeof(String))},
						{"A229", new Tuple<String, PROPERTYKEY, Type>("Space used", new PROPERTYKEY(){fmtid = Guid.Parse("9b174b35-40ff-11d2-a27e-00c04fc30871"), pid = 5}, typeof(String))},
						{"A230", new Tuple<String, PROPERTYKEY, Type>("EXIF version", new PROPERTYKEY(){fmtid = Guid.Parse("d35f743a-eb2e-47f2-a286-844132cb1427"), pid = 100}, typeof(String))},
						{"A231", new Tuple<String, PROPERTYKEY, Type>("Event", new PROPERTYKEY(){fmtid = Guid.Parse("14b81da1-0135-4d31-96d9-6cbfc9671a99"), pid = 18248}, typeof(String))},
						{"A232", new Tuple<String, PROPERTYKEY, Type>("Exposure bias", new PROPERTYKEY(){fmtid = Guid.Parse("14b81da1-0135-4d31-96d9-6cbfc9671a99"), pid = 37380}, typeof(String))},
						{"A233", new Tuple<String, PROPERTYKEY, Type>("Exposure program", new PROPERTYKEY(){fmtid = Guid.Parse("14b81da1-0135-4d31-96d9-6cbfc9671a99"), pid = 34850}, typeof(String))},
						{"A234", new Tuple<String, PROPERTYKEY, Type>("Exposure time", new PROPERTYKEY(){fmtid = Guid.Parse("14b81da1-0135-4d31-96d9-6cbfc9671a99"), pid = 33434}, typeof(String))},
						{"A235", new Tuple<String, PROPERTYKEY, Type>("F-stop", new PROPERTYKEY(){fmtid = Guid.Parse("14b81da1-0135-4d31-96d9-6cbfc9671a99"), pid = 33437}, typeof(String))},
						{"A236", new Tuple<String, PROPERTYKEY, Type>("Flash mode", new PROPERTYKEY(){fmtid = Guid.Parse("14b81da1-0135-4d31-96d9-6cbfc9671a99"), pid = 37385}, typeof(String))},
						{"A237", new Tuple<String, PROPERTYKEY, Type>("Focal length", new PROPERTYKEY(){fmtid = Guid.Parse("14b81da1-0135-4d31-96d9-6cbfc9671a99"), pid = 37386}, typeof(String))},
						{"A238", new Tuple<String, PROPERTYKEY, Type>("35mm focal length", new PROPERTYKEY(){fmtid = Guid.Parse("a0e74609-b84d-4f49-b860-462bd9971f98"), pid = 100}, typeof(String))},
						{"A239", new Tuple<String, PROPERTYKEY, Type>("ISO speed", new PROPERTYKEY(){fmtid = Guid.Parse("14b81da1-0135-4d31-96d9-6cbfc9671a99"), pid = 34855}, typeof(String))},
						{"A240", new Tuple<String, PROPERTYKEY, Type>("Lens maker", new PROPERTYKEY(){fmtid = Guid.Parse("e6ddcaf7-29c5-4f0a-9a68-d19412ec7090"), pid = 100}, typeof(String))},
						{"A241", new Tuple<String, PROPERTYKEY, Type>("Lens model", new PROPERTYKEY(){fmtid = Guid.Parse("e1277516-2b5f-4869-89b1-2e585bd38b7a"), pid = 100}, typeof(String))},
						{"A242", new Tuple<String, PROPERTYKEY, Type>("Light source", new PROPERTYKEY(){fmtid = Guid.Parse("14b81da1-0135-4d31-96d9-6cbfc9671a99"), pid = 37384}, typeof(String))},
						{"A243", new Tuple<String, PROPERTYKEY, Type>("Max aperture", new PROPERTYKEY(){fmtid = Guid.Parse("08f6d7c2-e3f2-44fc-af1e-5aa5c81a2d3e"), pid = 100}, typeof(String))},
						{"A244", new Tuple<String, PROPERTYKEY, Type>("Metering mode", new PROPERTYKEY(){fmtid = Guid.Parse("14b81da1-0135-4d31-96d9-6cbfc9671a99"), pid = 37383}, typeof(String))},
						{"A245", new Tuple<String, PROPERTYKEY, Type>("Orientation", new PROPERTYKEY(){fmtid = Guid.Parse("14b81da1-0135-4d31-96d9-6cbfc9671a99"), pid = 274}, typeof(String))},
						{"A246", new Tuple<String, PROPERTYKEY, Type>("People", new PROPERTYKEY(){fmtid = Guid.Parse("e8309b6e-084c-49b4-b1fc-90a80331b638"), pid = 100}, typeof(String))},
						{"A247", new Tuple<String, PROPERTYKEY, Type>("Program mode", new PROPERTYKEY(){fmtid = Guid.Parse("6d217f6d-3f6a-4825-b470-5f03ca2fbe9b"), pid = 100}, typeof(String))},
						{"A248", new Tuple<String, PROPERTYKEY, Type>("Saturation", new PROPERTYKEY(){fmtid = Guid.Parse("49237325-a95a-4f67-b211-816b2d45d2e0"), pid = 100}, typeof(String))},
						{"A249", new Tuple<String, PROPERTYKEY, Type>("Subject distance", new PROPERTYKEY(){fmtid = Guid.Parse("14b81da1-0135-4d31-96d9-6cbfc9671a99"), pid = 37382}, typeof(String))},
						{"A250", new Tuple<String, PROPERTYKEY, Type>("White balance", new PROPERTYKEY(){fmtid = Guid.Parse("ee3d3d8a-5381-4cfa-b13b-aaf66b5f4ec9"), pid = 100}, typeof(String))},
						{"A251", new Tuple<String, PROPERTYKEY, Type>("Priority", new PROPERTYKEY(){fmtid = Guid.Parse("9c1fcf74-2d97-41ba-b4ae-cb2e3661a6e4"), pid = 5}, typeof(String))},
						{"A252", new Tuple<String, PROPERTYKEY, Type>("Project", new PROPERTYKEY(){fmtid = Guid.Parse("39a7f922-477c-48de-8bc8-b28441e342e3"), pid = 100}, typeof(String))},
						{"A253", new Tuple<String, PROPERTYKEY, Type>("Channel number", new PROPERTYKEY(){fmtid = Guid.Parse("6d748de2-8d38-4cc3-ac60-f009b057c557"), pid = 7}, typeof(String))},
						{"A254", new Tuple<String, PROPERTYKEY, Type>("Episode name", new PROPERTYKEY(){fmtid = Guid.Parse("6d748de2-8d38-4cc3-ac60-f009b057c557"), pid = 2}, typeof(String))},
						{"A255", new Tuple<String, PROPERTYKEY, Type>("Closed captioning", new PROPERTYKEY(){fmtid = Guid.Parse("6d748de2-8d38-4cc3-ac60-f009b057c557"), pid = 12}, typeof(String))},
						{"A256", new Tuple<String, PROPERTYKEY, Type>("Rerun", new PROPERTYKEY(){fmtid = Guid.Parse("6d748de2-8d38-4cc3-ac60-f009b057c557"), pid = 13}, typeof(String))},
						{"A257", new Tuple<String, PROPERTYKEY, Type>("SAP", new PROPERTYKEY(){fmtid = Guid.Parse("6d748de2-8d38-4cc3-ac60-f009b057c557"), pid = 14}, typeof(String))},
						{"A258", new Tuple<String, PROPERTYKEY, Type>("Broadcast date", new PROPERTYKEY(){fmtid = Guid.Parse("4684fe97-8765-4842-9c13-f006447b178c"), pid = 100}, typeof(String))},
						{"A259", new Tuple<String, PROPERTYKEY, Type>("Program description", new PROPERTYKEY(){fmtid = Guid.Parse("6d748de2-8d38-4cc3-ac60-f009b057c557"), pid = 3}, typeof(String))},
						{"A260", new Tuple<String, PROPERTYKEY, Type>("Recording time", new PROPERTYKEY(){fmtid = Guid.Parse("a5477f61-7a82-4eca-9dde-98b69b2479b3"), pid = 100}, typeof(String))},
						{"A261", new Tuple<String, PROPERTYKEY, Type>("Station call sign", new PROPERTYKEY(){fmtid = Guid.Parse("6d748de2-8d38-4cc3-ac60-f009b057c557"), pid = 5}, typeof(String))},
						{"A262", new Tuple<String, PROPERTYKEY, Type>("Station name", new PROPERTYKEY(){fmtid = Guid.Parse("1b5439e7-eba1-4af8-bdd7-7af1d4549493"), pid = 100}, typeof(String))},
						{"A263", new Tuple<String, PROPERTYKEY, Type>("Summary", new PROPERTYKEY(){fmtid = Guid.Parse("560c36c0-503a-11cf-baa1-00004c752a9a"), pid = 2}, typeof(String))},
						{"A264", new Tuple<String, PROPERTYKEY, Type>("Snippets", new PROPERTYKEY(){fmtid = Guid.Parse("560c36c0-503a-11cf-baa1-00004c752a9a"), pid = 3}, typeof(String))},
						{"A265", new Tuple<String, PROPERTYKEY, Type>("Auto summary", new PROPERTYKEY(){fmtid = Guid.Parse("560c36c0-503a-11cf-baa1-00004c752a9a"), pid = 4}, typeof(String))},
						{"A266", new Tuple<String, PROPERTYKEY, Type>("Search ranking", new PROPERTYKEY(){fmtid = Guid.Parse("49691c90-7e17-101a-a91c-08002b2ecda9"), pid = 3}, typeof(String))},
						{"A267", new Tuple<String, PROPERTYKEY, Type>("Sensitivity", new PROPERTYKEY(){fmtid = Guid.Parse("f8d3f6ac-4874-42cb-be59-ab454b30716a"), pid = 100}, typeof(String))},
						{"A268", new Tuple<String, PROPERTYKEY, Type>("Shared with", new PROPERTYKEY(){fmtid = Guid.Parse("ef884c5b-2bfe-41bb-aae5-76eedf4f9902"), pid = 200}, typeof(String))},
						{"A269", new Tuple<String, PROPERTYKEY, Type>("Sharing status", new PROPERTYKEY(){fmtid = Guid.Parse("ef884c5b-2bfe-41bb-aae5-76eedf4f9902"), pid = 300}, typeof(String))},
						{"A270", new Tuple<String, PROPERTYKEY, Type>("Product name", new PROPERTYKEY(){fmtid = Guid.Parse("0cef7d53-fa64-11d1-a203-0000f81fedee"), pid = 7}, typeof(String))},
						{"A271", new Tuple<String, PROPERTYKEY, Type>("Product version", new PROPERTYKEY(){fmtid = Guid.Parse("0cef7d53-fa64-11d1-a203-0000f81fedee"), pid = 8}, typeof(String))},
						{"A272", new Tuple<String, PROPERTYKEY, Type>("Support link", new PROPERTYKEY(){fmtid = Guid.Parse("841e4f90-ff59-4d16-8947-e81bbffab36d"), pid = 6}, typeof(String))},
						{"A273", new Tuple<String, PROPERTYKEY, Type>("Source", new PROPERTYKEY(){fmtid = Guid.Parse("668cdfa5-7a1b-4323-ae4b-e527393a1d81"), pid = 100}, typeof(String))},
						{"A274", new Tuple<String, PROPERTYKEY, Type>("Start date", new PROPERTYKEY(){fmtid = Guid.Parse("48fd6ec8-8a12-4cdf-a03e-4ec5a511edde"), pid = 100}, typeof(String))},
						{"A275", new Tuple<String, PROPERTYKEY, Type>("Billing information", new PROPERTYKEY(){fmtid = Guid.Parse("d37d52c6-261c-4303-82b3-08b926ac6f12"), pid = 100}, typeof(String))},
						{"A276", new Tuple<String, PROPERTYKEY, Type>("Complete", new PROPERTYKEY(){fmtid = Guid.Parse("084d8a0a-e6d5-40de-bf1f-c8820e7c877c"), pid = 100}, typeof(String))},
						{"A277", new Tuple<String, PROPERTYKEY, Type>("Task owner", new PROPERTYKEY(){fmtid = Guid.Parse("08c7cc5f-60f2-4494-ad75-55e3e0b5add0"), pid = 100}, typeof(String))},
						{"A278", new Tuple<String, PROPERTYKEY, Type>("Total file size", new PROPERTYKEY(){fmtid = Guid.Parse("28636aa6-953d-11d2-b5d6-00c04fd918d0"), pid = 14}, typeof(String))},
						{"A279", new Tuple<String, PROPERTYKEY, Type>("Legal trademarks", new PROPERTYKEY(){fmtid = Guid.Parse("0cef7d53-fa64-11d1-a203-0000f81fedee"), pid = 9}, typeof(String))},
						{"A280", new Tuple<String, PROPERTYKEY, Type>("Video compression", new PROPERTYKEY(){fmtid = Guid.Parse("64440491-4c8b-11d1-8b70-080036b11a03"), pid = 10}, typeof(String))},
						{"A281", new Tuple<String, PROPERTYKEY, Type>("Directors", new PROPERTYKEY(){fmtid = Guid.Parse("64440492-4c8b-11d1-8b70-080036b11a03"), pid = 20}, typeof(String))},
						{"A282", new Tuple<String, PROPERTYKEY, Type>("Data rate", new PROPERTYKEY(){fmtid = Guid.Parse("64440491-4c8b-11d1-8b70-080036b11a03"), pid = 8}, typeof(String))},
						{"A283", new Tuple<String, PROPERTYKEY, Type>("Frame height", new PROPERTYKEY(){fmtid = Guid.Parse("64440491-4c8b-11d1-8b70-080036b11a03"), pid = 4}, typeof(String))},
						{"A284", new Tuple<String, PROPERTYKEY, Type>("Frame rate", new PROPERTYKEY(){fmtid = Guid.Parse("64440491-4c8b-11d1-8b70-080036b11a03"), pid = 6}, typeof(String))},
						{"A285", new Tuple<String, PROPERTYKEY, Type>("Frame width", new PROPERTYKEY(){fmtid = Guid.Parse("64440491-4c8b-11d1-8b70-080036b11a03"), pid = 3}, typeof(String))},
						{"A286", new Tuple<String, PROPERTYKEY, Type>("Total bitrate", new PROPERTYKEY(){fmtid = Guid.Parse("64440491-4c8b-11d1-8b70-080036b11a03"), pid = 43}, typeof(String))},
						//disabled these because I have no idea what they are
						//{"A287", new Tuple<String, PROPERTYKEY, Type>("", new PROPERTYKEY(){fmtid = Guid.Parse("1ce0d6bc-536c-4600-b0dd-7e0c66b350d5"), pid = 3}, typeof(String))},
						//{"A288", new Tuple<String, PROPERTYKEY, Type>("", new PROPERTYKEY(){fmtid = Guid.Parse("1ce0d6bc-536c-4600-b0dd-7e0c66b350d5"), pid = 4}, typeof(String))},
						//{"A289", new Tuple<String, PROPERTYKEY, Type>("", new PROPERTYKEY(){fmtid = Guid.Parse("1ce0d6bc-536c-4600-b0dd-7e0c66b350d5"), pid = 5}, typeof(String))},
						//{"A290", new Tuple<String, PROPERTYKEY, Type>("", new PROPERTYKEY(){fmtid = Guid.Parse("1ce0d6bc-536c-4600-b0dd-7e0c66b350d5"), pid = 6}, typeof(String))},
						//{"A291", new Tuple<String, PROPERTYKEY, Type>("Masters Keywords (debug)", new PROPERTYKEY(){fmtid = Guid.Parse("a4790b72-7113-4348-97ea-292bbc1f6770"), pid = 6}, typeof(String))},
						//{"A292", new Tuple<String, PROPERTYKEY, Type>("Masters Keywords (debug)", new PROPERTYKEY(){fmtid = Guid.Parse("a4790b72-7113-4348-97ea-292bbc1f6770"), pid = 5}, typeof(String))},
				};

		/// <summary>
		/// Convert and Collumns structure to LVCOLUMN (Native Listview Column)
		/// </summary>
		/// <param name="col">the column</param>
		/// <returns>resulting LVCOLUMN structure</returns>
		public static LVCOLUMN ToNativeColumn(this Collumns col) {
			LVCOLUMN column = new LVCOLUMN();
			column.mask = LVCF.LVCF_FMT | LVCF.LVCF_TEXT | LVCF.LVCF_WIDTH;
			column.cx = 100;
			column.pszText = col.Name;
			column.fmt = col.CollumnType == typeof(long) ? LVCFMT.RIGHT : LVCFMT.LEFT;
			return column;
		}
		public static List<Collumns> AvailableColumns(this ShellView view) {
			return AllColumnsPKeys.Select(s => new Collumns() { ID = s.Key, CollumnType = s.Value.Item3, pkey = s.Value.Item2, Name = s.Value.Item1, Width = 100, IsColumnHandler = false }).ToList();
		}
	}

	public static class SystemProperties {
		public static PROPERTYKEY FileSize = new PROPERTYKEY() { fmtid = Guid.Parse("b725f130-47ef-101a-a5f1-02608c9eebac"), pid = 12 };
		public static PROPERTYKEY LinkTarget = new PROPERTYKEY() { fmtid = Guid.Parse("b9b4b3fc-2b51-4a42-b5d8-324146afcf25"), pid = 2 };
	}
}
