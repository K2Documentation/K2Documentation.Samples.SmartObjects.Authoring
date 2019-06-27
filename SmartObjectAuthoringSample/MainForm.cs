using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SourceCode.SmartObjects.Management;
using SourceCode.Hosting.Client.BaseAPI;
using SourceCode.SmartObjects.Authoring;
using SourceCode.SmartObjects.Client;

namespace SmartObjectAuthoringSample
{
    public partial class MainForm : Form
    {
        private SmartObjectManagementServer _smoManagementServer;
	    private SmartObjectClientServer _smoClientServer;
	    private SCConnectionStringBuilder _connectionString;

	    private const string _smartboxGuid = "e5609413-d844-4325-98c3-db3cacbd406d";
	    private const string _adProviderService = "d591c983-ba0a-487e-9f08-a8748558d6b9";
	    private const string _serverName = "localhost";
	    private const uint _serverPort = 5555;

	    private const string _smoEmployees = "Employees";
	    private const string _smoRegions = "Regions";
	    private const string _smoDepartments = "Departments";

        public MainForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            BuildSmartObjects();
        }


	    private void BuildSmartObjects()
	    {
	        try
	        {
                toolStripStatusLabel1.Text = "Creating SmartObjects...";
                statusStrip1.Update();
                
                InitializeConnectionString();

		        SmartObjectDefinitionsPublish publishSmo = new SmartObjectDefinitionsPublish();

		        publishSmo.SmartObjects.Add(this.CreateSmartObject(_smoEmployees));
                publishSmo.SmartObjects.Add(this.CreateSmartObject(_smoRegions));
		        publishSmo.SmartObjects.Add(this.CreateSmartObject(_smoDepartments));


                // Publish SmartObjects before creating associations
                PublishSmartObjects(publishSmo);

                // Re-intialize the SmartObject definitions
                publishSmo.Dispose();
                publishSmo = new SmartObjectDefinitionsPublish();

                // Create SmartObject associations
		        publishSmo.SmartObjects.Add(this.CreateAssociation(_smoEmployees, _smoRegions, SourceCode.SmartObjects.Authoring.AssociationType.OneToMany));
		        publishSmo.SmartObjects.Add(this.CreateAssociation(_smoDepartments, _smoEmployees, SourceCode.SmartObjects.Authoring.AssociationType.ManyToOne));
		        publishSmo.SmartObjects.Add(this.CreateAssociation(_smoRegions, _smoDepartments, SourceCode.SmartObjects.Authoring.AssociationType.ManyToOne));

		        // Publish SmartObjects with associations
		        PublishSmartObjects(publishSmo);

                // Re-intialize the SmartObject definitions
                publishSmo.Dispose();
                publishSmo = new SmartObjectDefinitionsPublish();

                publishSmo.SmartObjects.Add(CreateServiceJoin(_smoEmployees, _smoRegions));

                // Publish association SmO
                PublishSmartObjects(publishSmo);

                toolStripStatusLabel1.Text = "SmartObject creation successful.";
	        }

	        catch (Exception ex)
	        {
		        MessageBox.Show(ex.Message);
	        }

	    }

	    private SmartObjectDefinition CreateSmartObject(string smoName)
	    {
	        try
	        {
                // Delete the smartobject if it already exists
                this.DeleteSmartObject(smoName);

                toolStripStatusLabel1.Text = ("Creating SmartObject properties for '" + smoName + "'");
                statusStrip1.Update();
		        ManagementServerConnect();

		        // Get SmartBox service instance
		        ServiceInstance serviceInstance = ServiceInstance.Create(_smoManagementServer.GetServiceInstanceForExtend(new Guid(_smartboxGuid), string.Empty));
		        ExtendObject extendObject = serviceInstance.GetCreateExtender();

                extendObject.Name = smoName;
                extendObject.Metadata.DisplayName = smoName;

		        // Create 'id' property
		        ExtendObjectProperty idProperty = new ExtendObjectProperty();
		        idProperty.Name = "ID";
		        idProperty.Metadata.DisplayName = idProperty.Name;
		        idProperty.Type = PropertyDefinitionType.Autonumber;
		        idProperty.ExtendType = ExtendPropertyType.UniqueIdAuto;

		        // Create 'name' property
		        ExtendObjectProperty nameProperty = new ExtendObjectProperty();
		        nameProperty.Name = "Name";
		        nameProperty.Metadata.DisplayName = nameProperty.Name;
		        nameProperty.Type = PropertyDefinitionType.Text;

                // Create other properties here as needed
                // Add the new properties below

		        // Add properties
		        extendObject.Properties.Add(idProperty);
		        extendObject.Properties.Add(nameProperty);

		        SmartObjectDefinition smoDefinition = new SmartObjectDefinition();

		        // Create SmartObject Definition
		        smoDefinition.Create(extendObject);
		        smoDefinition.AddDeploymentCategory("Test SmartObjects");

		        smoDefinition.Build();

		        return smoDefinition;
	        }

            catch (Exception ex)
	        {
		        throw ex;
	        }

	        finally
	        {
		        ManagementServerCloseConnection();
	        }
	    }


        private SmartObjectDefinition CreateAssociation(string smoName, string associationName, SourceCode.SmartObjects.Authoring.AssociationType associationType)
	    {
	        try
	        {
                toolStripStatusLabel1.Text = ("Creating association between '" + smoName + "' and '" + associationName + "'");
                statusStrip1.Update();
		        ManagementServerConnect();

                string smartObjectXml = _smoManagementServer.GetSmartObjectDefinition(smoName);
		        SmartObjectDefinition smoDefinition = SmartObjectDefinition.Create(smartObjectXml);

		        string associationXml = _smoManagementServer.GetAssociationSmartObject(associationName);
		        AssociationSmartObject associationDefinition = AssociationSmartObject.Create(associationXml);

                smoDefinition.AddAssociation(associationDefinition, associationDefinition.Properties[1], smoDefinition.Properties[1], associationType, "test association");
		        smoDefinition.AddDeploymentCategory("Test SmartObjects");

		        smoDefinition.Build();

		        return smoDefinition;

	        }

            catch (Exception ex)
	        {
		    throw ex;
	        }

	        finally
	        {
    		    ManagementServerCloseConnection();
	        }
	    }

	    private SmartObjectDefinition CreateServiceJoin(string smoName1, string smoName2)
	    {
	        try
	        {
		        // Delete the smartobject if it already exists
                this.DeleteSmartObject(smoName1 + smoName2);

                toolStripStatusLabel1.Text = ("Creating ServiceJoin SmartObject '" + smoName1 + " " + smoName2 + "'");
                statusStrip1.Update();

		        ManagementServerConnect();

		        // Get first serviceobject

                ServiceInstance serviceInstance1 = ServiceInstance.Create(_smoManagementServer.GetServiceInstanceForExtend(new Guid(_smartboxGuid), smoName1));
		        ServiceObject serviceObject1 = serviceInstance1.ServiceObjects[0];

		        if (serviceObject1 == null)
		        {
		            throw new Exception("Serviceobject does not exist. " + smoName1);
		        }

		        // Get second serviceobject

                ServiceInstance serviceInstance2 = ServiceInstance.Create(_smoManagementServer.GetServiceInstanceForExtend(new Guid(_smartboxGuid), smoName2));
		        ServiceObject serviceObject2 = serviceInstance2.ServiceObjects[0];

		        if (serviceObject2 == null)
		        {
                    throw new Exception("Serviceobject does not exist. " + smoName2);
		        }

		        // Create SmartObjectDefinition

		        SmartObjectDefinition smoDefinition = new SmartObjectDefinition();
                smoDefinition.Name = smoName1 + smoName2;
                smoDefinition.Metadata.DisplayName = smoName1 + " " + smoName2;

		        // Get Getlist servicemethod

		        ServiceObjectMethod method = serviceObject1.Methods["GetList"];

		        if (method == null)
		            throw new Exception("GetList method does not exist on " + serviceObject1.Name);

		        // Create SmartMethodDefinition

		        SmartMethodDefinition soMethod = new SmartMethodDefinition();
		        soMethod.Name = method.Name;
		        soMethod.Metadata.DisplayName = method.DisplayName;

		        smoDefinition.Methods.Add(soMethod);

		        // Map the first ServiceObject
                string executionBlockName = soMethod.Map(method);

		        foreach (ServiceObjectProperty soProperty in serviceObject1.Properties)
		        {
		            SmartPropertyDefinition smartProperty = new SmartPropertyDefinition();
		            smartProperty.Name = soProperty.Name + "_1";
		            smartProperty.Metadata.DisplayName = soProperty.Name + "_1";
		            smoDefinition.Properties.Add(smartProperty);

		            smoDefinition.MapProperty(smartProperty, soProperty, executionBlockName, method.Name);
		        }

		        // Map the second ServiceObject

		        method = serviceObject2.Methods["GetList"];

		        if (method == null)
		            throw new Exception("GetList method does not exist on " + serviceObject1.Name);

		        executionBlockName = soMethod.Map(method);

		        foreach (ServiceObjectProperty soProperty in serviceObject2.Properties)
		        {
		            SmartPropertyDefinition smartProperty = new SmartPropertyDefinition();
                    smartProperty.Name = soProperty.Name + "_2";
		            smartProperty.Metadata.DisplayName = soProperty.Name + "_2";
		            smoDefinition.Properties.Add(smartProperty);

                    smoDefinition.MapProperty(smartProperty, soProperty, executionBlockName, method.Name);
		        }

		        ServiceJoinDetails joinDetails = smoDefinition.Methods[0].JoinDetails;
		        joinDetails.From = soMethod.ExecutionBlocks[0].ServiceInstance.ServiceObjects[0];

		        ServiceJoin serviceJoin = new ServiceJoin();
		        serviceJoin.From = soMethod.ExecutionBlocks[0].ServiceInstance.ServiceObjects[0]; ;
		        serviceJoin.To = soMethod.ExecutionBlocks[1].ServiceInstance.ServiceObjects[0]; ;

		        serviceJoin.AddCondition("Condition", serviceObject1.Properties[1], serviceObject2.Properties[1]);

		        joinDetails.ServiceJoins.Add(serviceJoin);

                smoDefinition.AddDeploymentCategory("Test SmartObjects");

		        smoDefinition.Build();
		        return smoDefinition;

	        }

	        catch (Exception ex)
	        {
		        throw ex;
	        }

	        finally
	        {
		        ManagementServerCloseConnection();
	        }
	    }


        // The following function is not used but is included here for reference
        private SmartObjectDefinition CreateLookup(string smoName)
	    {
	        try
	        {
		        ManagementServerConnect();
                string smoDefinitionXml = _smoManagementServer.GetSmartObjectDefinition(smoName);

		        SmartObjectDefinition smoDefinition = SmartObjectDefinition.Create(smoDefinitionXml);
		        smoDefinition.Lookup.LookupDisplayProperties.Add(smoDefinition.Properties[1]);
		        smoDefinition.Lookup.LookupKeyProperties.Add(smoDefinition.Properties[0]);

		        smoDefinition.Build();
		        return smoDefinition;
	        }

	        catch (Exception ex)
	        {
		        throw ex;
	        }

	        finally
	        {
		        ManagementServerCloseConnection();
	        }
	    }

        private void DeleteSmartObject(string smoName)
	    {
	        try
	        {
		        ManagementServerConnect();
                SmartObjectExplorer checkSmartObjectExist = _smoManagementServer.GetSmartObjects(smoName);

		        if (checkSmartObjectExist.SmartObjects.Count > 0)
		        {
                    _smoManagementServer.DeleteSmartObject(smoName, true);
		        }
	        }

	        catch (Exception ex)
	        {
		        MessageBox.Show(ex.Message);
	        }

	        finally
	        {
		        ManagementServerCloseConnection();
	        }
	    }

	    private void PublishSmartObjects(SmartObjectDefinitionsPublish publishSmo)
	    {
	        try
	        {
                toolStripStatusLabel1.Text = "Publishing SmartObjects...";
                statusStrip1.Update();
		        ManagementServerConnect();
		        _smoManagementServer.PublishSmartObjects(publishSmo.ToPublishXml());
	        }

	        catch (Exception ex)
	        {
		        MessageBox.Show(ex.Message);
	        }

            finally
	        {
		        ManagementServerCloseConnection();
	        }
	    }

	    #region Server Access

	    private void ManagementServerConnect()
	    {
	        if (_smoManagementServer == null)
	        {
		        _smoManagementServer = new SmartObjectManagementServer();
	        }
	        if (_smoManagementServer.Connection == null)
	        {
		        _smoManagementServer.CreateConnection();
	        }
	        if (!_smoManagementServer.Connection.IsConnected)
	        {
                _smoManagementServer.Connection.Open(_connectionString.ConnectionString);
	        }

	    }

	    private void ManagementServerCloseConnection()
	    {
	        if (_smoManagementServer != null)
	        {
		        if (_smoManagementServer.Connection != null)
		        {
		            if (_smoManagementServer.Connection.IsConnected)
		            {
			            _smoManagementServer.Connection.Close();
		            }
		        }
	        }

	    }

	    private void ClientServerConnect()
	    {
	        if (_smoClientServer == null)
	        {
		        _smoClientServer = new SmartObjectClientServer();
	        }
	        if (_smoClientServer.Connection == null)
	        {
		        _smoClientServer.CreateConnection();
	        }
	        if (!_smoClientServer.Connection.IsConnected)
	        {
		        _smoClientServer.Connection.Open(_connectionString.ConnectionString);
	        }
	    }

	    private void ClientServerCloseConnection()
	    {
            if (_smoClientServer != null)
            {
		    if (_smoClientServer.Connection != null)
		    {
		        if (_smoClientServer.Connection.IsConnected)
                {
                    _smoClientServer.Connection.Close();
                }
		    }
	        }

	    }

	    private void InitializeConnectionString()
	    {
	        _connectionString = new SCConnectionStringBuilder();
	        _connectionString.Host = _serverName;
	        _connectionString.Port = _serverPort;
	        _connectionString.IsPrimaryLogin = true;
	        _connectionString.Integrated = true;
	    }

	    #endregion


    }
}
