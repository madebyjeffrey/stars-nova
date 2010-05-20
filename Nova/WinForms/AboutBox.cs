#region Copyright Notice
// ============================================================================
// Copyright (C) 2008 Ken Reed
// Copyright (C) 2009, 2010 The Stars-Nova Project
//
// This file is part of Stars! Nova.
// See <http://sourceforge.net/projects/stars-nova/>.
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License version 2 as
// published by the Free Software Foundation.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program. If not, see <http://www.gnu.org/licenses/>
// ===========================================================================
#endregion

#region Module Description
// ===========================================================================
// This dialog displays the "About" box.
// ===========================================================================
#endregion

using System;
using System.Windows.Forms;
using System.Reflection;

namespace Nova.WinForms
{
    partial class AboutBox : Form
    {
        public AboutBox()
        {
            InitializeComponent();

            this.logoPictureBox.Image = Nova.Properties.Resources.Nova;

            this.Text = String.Format("About {0}", AssemblyTitle);
            this.labelProductName.Text = AssemblyProduct;
            this.labelVersion.Text = String.Format("Version {0}", AssemblyVersion);
            this.Description.Text =
                "Copyright � 2008 Ken Reed" + Environment.NewLine +
                "Copyright � 2009, 2010 The Stars-Nova Project" + Environment.NewLine +
                "" + Environment.NewLine +
                AssemblyProduct + " is licensed under two separate licenses for code and content." + Environment.NewLine +
                "" + Environment.NewLine +
                "Content (images, documentation and other media) is licensed under the " +
                "Creative Commons Attribution-ShareAlike 3.0 Unported license. Content " +
                "includes images, music, sounds, text and game content such as components " +
                "and races. You should have received a copy of the Creative Commons " +
                "Attribution-ShareAlike 3.0 Unported license along with this program. " +
                "If not, visit <http://creativecommons.org/licenses/by-sa/3.0/> or send a " +
                "letter to Creative Commons, 559 Nathan Abbott Way, Stanford, " +
                "California 94305, USA." + Environment.NewLine +
                "" + Environment.NewLine +
                "Everything else is licensed under the GNU General Public License " +
                "version 2. This includes, but is not limited to, source code, executable " +
                "and object code. You should have received a copy of the GNU General " +
                "Public License version 2 along with this program. If not, visit " +
                "<http://www.gnu.org/licenses/> or send a letter to the " +
                "Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, " +
                "MA 02110-1301, USA." + Environment.NewLine +
                "" + Environment.NewLine +
                "This program is distributed in the hope that it will be useful, but " +
                "WITHOUT ANY WARRANTY; without even the implied warranty of " +
                "MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General " +
                "Public License version 2 for more details.";
        }

        #region Assembly Attribute Accessors


        /// <summary>
        /// Get the assembly title.
        /// </summary>
        public string AssemblyTitle
        {
            get
            {
                // Get all Title attributes on this assembly
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                // If there is at least one Title attribute
                if (attributes.Length > 0)
                {
                    // Select the first one
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    // If it is not an empty string, return it
                    if (titleAttribute.Title != "")
                        return titleAttribute.Title;
                }
                // If there was no Title attribute, or if the Title attribute was the empty string, return the .exe name
                return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
            }
        }


        /// <summary>
        /// Get the assembly version.
        /// </summary>
        public string AssemblyVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }


        /// <summary>
        /// Get the assembly description.
        /// </summary>
        public string AssemblyDescription
        {
            get
            {
                // Get all Description attributes on this assembly
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                // If there aren't any Description attributes, return an empty string
                if (attributes.Length == 0)
                    return "";
                // If there is a Description attribute, return its value
                return ((AssemblyDescriptionAttribute)attributes[0]).Description;
            }
        }


        /// <summary>
        /// Get the assembly product.
        /// </summary>
        public string AssemblyProduct
        {
            get
            {
                // Get all Product attributes on this assembly
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                // If there aren't any Product attributes, return an empty string
                if (attributes.Length == 0)
                    return "";
                // If there is a Product attribute, return its value
                return ((AssemblyProductAttribute)attributes[0]).Product;
            }
        }


        /// <summary>
        /// Get the assembly copyright.
        /// </summary>
        public string AssemblyCopyright
        {
            get
            {
                // Get all Copyright attributes on this assembly
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                // If there aren't any Copyright attributes, return an empty string
                if (attributes.Length == 0)
                    return "";
                // If there is a Copyright attribute, return its value
                return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            }
        }


        /// <summary>
        /// Get the assembly company
        /// </summary>
        public string AssemblyCompany
        {
            get
            {
                // Get all Company attributes on this assembly
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                // If there aren't any Company attributes, return an empty string
                if (attributes.Length == 0)
                    return "";
                // If there is a Company attribute, return its value
                return ((AssemblyCompanyAttribute)attributes[0]).Company;
            }
        }
        #endregion
    
    }//AboutBox

}//namespace
