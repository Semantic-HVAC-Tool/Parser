using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.ApplicationServices;
using RestSharp;
using System.Net.Http;
using Autodesk.Revit.DB.Analysis;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;

namespace RevitToRDFConverter
{
    class RelatedPorts
    {
        public static StringBuilder FamilyInstanceConnectors(FamilyInstance component, string revitID  ,string componentID, StringBuilder sb)
        {
            //Port type
            ConnectorSet connectorSet = component.MEPModel.ConnectorManager.Connectors;

            string connectorID;
            string connectorDirectionID;
            string connectorDirection;
            double connectorOuterDiameter;
            string connectedConnectorID;
            string connectedConnectorDirection;
            string connectorOuterDiameterID;
            string connectorWidthID;
            string connectorHeightID;
            double connectorWidth;
            double connectorHeight;
            string connectedComponentID;
            string connectorDirectionVectorZID;
            string connectorDirectionVectorZ;
            string crossSectionalAreaID;
            double crossSectionalArea;

            foreach (Connector connector in connectorSet)
            {
    
                if (Domain.DomainHvac == connector.Domain || Domain.DomainPiping == connector.Domain)
                {

                    //Type
                    connectorID = componentID + "-" + connector.Id;
                    sb.Append($"inst:{componentID} fso:hasPort inst:{connectorID} ." + "\n"
                        + $"inst:{connectorID} a fso:Port ." + "\n");

                    //FlowDirection
                    connectorDirectionID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                    connectorDirection = connector.Direction.ToString();
                    connectorDirectionVectorZID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                    connectorDirectionVectorZ = connector.CoordinateSystem.BasisZ.ToString();

                    sb.Append($"inst:{connectorID} fpo:hasFlowDirection inst:{connectorDirectionID} ." + "\n"
                                            + $"inst:{connectorDirectionID} a fpo:FlowDirection ." + "\n"
                                            + $"inst:{connectorDirectionID} fpo:hasValue '{connectorDirection}'^^xsd:string ." + "\n"
                                            + $"inst:{connectorID} ex:hasFlowDirectionVectorZ inst:{connectorDirectionVectorZID} ." + "\n"
                                            + $"inst:{connectorDirectionVectorZID} fpo:hasValue '{connectorDirectionVectorZ}'^^xsd:string ." + "\n"
                                            );

                    //Size
                    if (connector.Shape.ToString() == "Round")
                    {
                        connectorOuterDiameterID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                        connectorOuterDiameter = UnitUtils.ConvertFromInternalUnits(connector.Radius * 2, UnitTypeId.Meters);
                        sb.Append($"inst:{connectorID} fpo:hasOuterDiameter inst:{connectorOuterDiameterID} ." + "\n" +
                            $"inst:{connectorOuterDiameterID} a fpo:OuterDiameter ." + "\n" +
                            $"inst:{connectorOuterDiameterID} fpo:hasValue '{connectorOuterDiameter}'^^xsd:double ." + "\n" +
                            $"inst:{connectorOuterDiameterID} fpo:hasUnit 'Meter'^^xsd:string  ." + "\n");

                        crossSectionalAreaID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                        crossSectionalArea = Math.PI * Math.Pow(UnitUtils.ConvertFromInternalUnits(connector.Radius, UnitTypeId.Meters), 2);
                        sb.Append($"inst:{connectorID} fpo:hasCrossSectionalArea inst:{crossSectionalAreaID} ." + "\n" +
                            $"inst:{crossSectionalAreaID} a fpo:CrossSectionalArea ." + "\n" +
                            $"inst:{crossSectionalAreaID} fpo:hasValue '{crossSectionalArea}'^^xsd:double ." + "\n" +
                            $"inst:{crossSectionalAreaID} fpo:hasUnit 'Meter'^^xsd:string  ." + "\n");

                    }

                    if (connector.Shape.ToString() == "Rectangular")
                    {
                        connectorWidthID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                        connectorHeightID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                        connectorHeight = UnitUtils.ConvertFromInternalUnits(connector.Height, UnitTypeId.Meters);
                        connectorWidth = UnitUtils.ConvertFromInternalUnits(connector.Width, UnitTypeId.Meters);
                        sb.Append($"inst:{connectorID} fpo:hasHeight inst:{connectorHeightID} ." + "\n" +
                            $"inst:{connectorHeightID} a fpo:Heigth ." + "\n" +
                            $"inst:{connectorHeightID} fpo:hasValue '{connectorHeight}'^^xsd:double ." + "\n" +
                            $"inst:{connectorHeightID} fpo:hasUnit 'Meter'^^xsd:string  ." + "\n" +
                            $"inst:{connectorID} fpo:hasWidth inst:{connectorWidthID} ." + "\n" +
                            $"inst:{connectorWidthID} a fpo:Width ." + "\n" +
                            $"inst:{connectorWidthID} fpo:hasValue '{connectorWidth}'^^xsd:double ." + "\n" +
                            $"inst:{connectorWidthID} fpo:hasUnit 'Meter'^^xsd:string  ." + "\n");

                        crossSectionalAreaID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                        crossSectionalArea = connectorHeight * connectorWidth;
                        sb.Append($"inst:{connectorID} fpo:hasCrossSectionalArea inst:{crossSectionalAreaID} ." + "\n" +
                            $"inst:{crossSectionalAreaID} a fpo:CrossSectionalArea ." + "\n" +
                            $"inst:{crossSectionalAreaID} fpo:hasValue '{crossSectionalArea}'^^xsd:double ." + "\n" +
                            $"inst:{crossSectionalAreaID} fpo:hasUnit 'Meter'^^xsd:string  ." + "\n");


                    }

                    if (connector.Flow != null)
                    {
                        //Flow rate
                        string flowID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                        double flowValue = UnitUtils.ConvertFromInternalUnits(connector.Flow, UnitTypeId.LitersPerSecond);
                        sb.Append($"inst:{connectorID} fpo:hasFlowRate inst:{flowID} ." + "\n"
                         + $"inst:{flowID} a fpo:FlowRate ." + "\n"
                         + $"inst:{flowID} fpo:hasValue '{flowValue}'^^xsd:double ." + "\n"
                         + $"inst:{flowID} fpo:hasUnit 'Liters per second'^^xsd:string ." + "\n");
                    }

                    //if (connector.VelocityPressure != null)
                    //{
                    //    //Flow rate
                    //    string velocityID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                    //    double velocityValue = UnitUtils.ConvertFromInternalUnits(connector.VelocityPressure, UnitTypeId.MetersPerSecond);
                    //    sb.Append($"inst:{connectorID} fpo:velocity inst:{velocityID} ." + "\n"
                    //     + $"inst:{velocityID} a fpo:Velocity ." + "\n"
                    //     + $"inst:{velocityID} fpo:hasValue '{velocityValue}'^^xsd:double ." + "\n"
                    //     + $"inst:{velocityID} fpo:hasUnit 'Meters per second'^^xsd:string ." + "\n");
                    //}

                        //Port relationship to other ports
                        ConnectorSet joinedconnectors = connector.AllRefs;
                    if (connectorDirection == "Out")
                    {
                        //if (component.LookupParameter("Flow") != null)
                        //{
                        //    //Flow rate
                        //    string flowID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                        //    double flowValue = UnitUtils.ConvertFromInternalUnits(component.LookupParameter("Flow").AsDouble(), UnitTypeId.LitersPerSecond);
                        //    sb.Append($"inst:{connectorID} fpo:hasFlowRate inst:{flowID} ." + "\n"
                        //     + $"inst:{flowID} a fpo:FlowRate ." + "\n"
                        //     + $"inst:{flowID} fpo:hasValue '{flowValue}'^^xsd:double ." + "\n"
                        //     + $"inst:{flowID} fpo:hasUnit 'Liters per second'^^xsd:string ." + "\n");
                        //}

                        if (component.LookupParameter("FSC_nomPressureDrop") != null)
                        {
                            //Water side pressure drop
                            string pressureDropID = System.Guid.NewGuid().ToString().Replace(' ', '-'); ;
                            double pressureDropValue = UnitUtils.ConvertFromInternalUnits(component.LookupParameter("FSC_nomPressureDrop").AsDouble(), UnitTypeId.Pascals);
                            sb.Append($"inst:{connectorID} fpo:hasPressureDrop inst:{pressureDropID} ." + "\n"
                           + $"inst:{pressureDropID} a fpo:PressureDrop ." + "\n"
                           + $"inst:{pressureDropID} fpo:hasValue '{pressureDropValue}'^^xsd:double ." + "\n"
                           + $"inst:{pressureDropID} fpo:hasUnit 'Pascal'^^xsd:string ." + "\n");
                        }

                        //if (component.LookupParameter("FSC_flow") != null)
                        //{
                        //    //FlowRate
                        //    string flowrateID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                        //    double flowrateValue = UnitUtils.ConvertFromInternalUnits(component.LookupParameter("FSC_flow").AsDouble(), UnitTypeId.LitersPerSecond);
                        //    sb.Append($"inst:{connectorID} fpo:hasFlowRate inst:{flowrateID} ." + "\n"
                        //     + $"inst:{flowrateID} a fpo:FlowRate ." + "\n"
                        //     + $"inst:{flowrateID} fpo:hasValue  '{flowrateValue}'^^xsd:double ." + "\n"
                        //     + $"inst:{flowrateID} fpo:hasUnit  'm3/h'^^xsd:string ." + "\n");
                        //}
                        if (component.LookupParameter("Reynolds Number") != null)
                        {
                            //Reynolds number
                            string reynoldsID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                            double reynoldsValue = component.LookupParameter("Reynolds Number").AsDouble();
                            sb.Append($"inst:{connectorID} fpo:hasReynoldsNumber inst:{reynoldsID} ." + "\n"
                             + $"inst:{reynoldsID} a fpo:ReynoldsNumber ." + "\n"
                             + $"inst:{reynoldsID} fpo:hasValue '{reynoldsValue}'^^xsd:double ." + "\n");
                        }
                        if (component.LookupParameter("Friction Factor") != null)
                        {
                            //frictionFactor 
                            string frictionFactorID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                            double frictionFactorValue = component.LookupParameter("Friction Factor").AsDouble();
                            sb.Append($"inst:{connectorID} fpo:hasFrictionFactor inst:{frictionFactorID} ." + "\n"
                             + $"inst:{frictionFactorID} a fpo:FrictionFactor ." + "\n"
                             + $"inst:{frictionFactorID} fpo:hasValue '{frictionFactorValue}'^^xsd:double ." + "\n");
                        }

                        //if (component.LookupParameter("Velocity") != null)
                        //{
                        //    //Flow Velocity
                        //    string velocityID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                        //    double velocityValue = UnitUtils.ConvertFromInternalUnits(component.LookupParameter("Velocity").AsDouble(), UnitTypeId.MetersPerSecond);
                        //    sb.Append($"inst:{connectorID} fpo:velocity inst:{velocityID} ." + "\n"
                        //     + $"inst:{velocityID} a fpo:Velocity ." + "\n"
                        //     + $"inst:{velocityID} fpo:hasValue '{velocityValue}'^^xsd:double ." + "\n"
                        //     + $"inst:{velocityID} fpo:hasUnit 'Meters per second'^^xsd:string ." + "\n");
                        //}

                        //if (component.LookupParameter("Pressure Drop") != null)
                        //{
                        //    //Pressure Drop
                        //    string pressureDropID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                        //    double pressureDropValue = UnitUtils.ConvertFromInternalUnits(component.LookupParameter("Pressure Drop").AsDouble(), UnitTypeId.Pascals);
                        //    sb.Append($"inst:{connectorID} fpo:hasPressureDrop inst:{pressureDropID} ." + "\n"
                        //     + $"inst:{pressureDropID} a fpo:PressureDrop ." + "\n"
                        //     + $"inst:{pressureDropID} fpo:hasValue '{pressureDropValue}'^^xsd:double ." + "\n"
                        //     + $"inst:{pressureDropID} fpo:hasUnit 'Pascal'^^xsd:string ." + "\n");
                        //};

                        //if (component.LookupParameter("Pressure loss") != null)
                        //{
                        //    //Pressure Drop
                        //    string pressureDropID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                        //    double pressureDropValue = UnitUtils.ConvertFromInternalUnits(component.LookupParameter("Pressure loss").AsDouble(), UnitTypeId.Pascals);
                        //    sb.Append($"inst:{connectorID} fpo:hasPressureDrop inst:{pressureDropID} ." + "\n"
                        //     + $"inst:{pressureDropID} a fpo:PressureDrop ." + "\n"
                        //     + $"inst:{pressureDropID} fpo:hasValue '{pressureDropValue}'^^xsd:double ." + "\n"
                        //     + $"inst:{pressureDropID} fpo:hasUnit 'Pascal'^^xsd:string ." + "\n");
                        //};



                        if (component.LookupParameter("Flow State") != null)
                            {
                            //Flow State
                            string flowStateID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                            string flowStateValue = component.LookupParameter("Flow State").AsValueString();
                            sb.Append($"inst:{connectorID} fpo:hasFlowState inst:{flowStateID} ." + "\n"
                             + $"inst:{flowStateID} a fpo:FlowState ." + "\n"
                             + $"inst:{flowStateID} fpo:hasValue '{flowStateValue}'^^xsd:string ." + "\n"
                             + $"inst:{flowStateID} fpo:hasUnit 'Pascal'^^xsd:string ." + "\n");
                        }


                       

                            foreach (Connector connectedConnector in joinedconnectors)
                        {
                            connectedConnectorID = connectedConnector.Owner.UniqueId.ToString() + "-" + connectedConnector.Id.ToString();
                            connectedComponentID = connectedConnector.Owner.UniqueId.ToString();

                            if (Domain.DomainHvac == connectedConnector.Domain && connector.DuctSystemType == DuctSystemType.SupplyAir
                            || (Domain.DomainPiping == connectedConnector.Domain && connector.PipeSystemType == PipeSystemType.SupplyHydronic))
                            {
                                connectedConnectorDirection = connectedConnector.Direction.ToString();

                                sb.Append($"inst:{connectorID} fso:suppliesFluidTo inst:{connectedConnectorID} ." + "\n"
                                    + $"inst:{connectedConnectorID} a fso:Port ." + "\n" 
                                    + $"inst:{componentID} fso:feedsFluidTo inst:{connectedComponentID} ." + "\n");

                            }
                            if (Domain.DomainHvac == connectedConnector.Domain && connector.DuctSystemType == DuctSystemType.ReturnAir
                            || Domain.DomainPiping == connectedConnector.Domain && connector.PipeSystemType == PipeSystemType.ReturnHydronic)
                            {
                                connectedConnectorDirection = connectedConnector.Direction.ToString();

                                sb.Append($"inst:{connectorID} fso:returnsFluidTo inst:{connectedConnectorID} ." + "\n"
                                    + $"inst:{connectedConnectorID} a fso:Port ." + "\n" 
                                    + $"inst:{componentID} fso:feedsFluidTo inst:{connectedComponentID} ." + "\n"
);

                            }



                        }
                    }
                }
            }

            return sb;
        }

        public static StringBuilder HeatExchangerConnectors(FamilyInstance component, string componentID, StringBuilder sb)
        {
            //Port type
            ConnectorSet connectorSet = component.MEPModel.ConnectorManager.Connectors;

            string connectorID;
            string connectorDirectionID;
            string connectorDirection;
            double connectorOuterDiameter;
            string connectedConnectorID;
            string connectedConnectorDirection;
            string connectorOuterDiameterID;
            string connectedComponentID;
            string connectorDirectionVectorZID;
            string connectorDirectionVectorZ;
            string crossSectionalAreaID;
            double crossSectionalArea;

            foreach (Connector connector in connectorSet)
            {
                if (Domain.DomainHvac == connector.Domain || Domain.DomainPiping == connector.Domain)
                {
                    //Type
                    connectorID = componentID + "-" + connector.Id;
                    sb.Append($"inst:{componentID} fso:hasPort inst:{connectorID} ." + "\n"
                        + $"inst:{connectorID} a fso:Port ." + "\n");

                    //FlowDirection
                    connectorDirectionID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                    connectorDirection = connector.Direction.ToString();
                    connectorDirectionVectorZID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                    connectorDirectionVectorZ = connector.CoordinateSystem.BasisZ.ToString();

                    sb.Append($"inst:{connectorID} fpo:hasFlowDirection inst:{connectorDirectionID} ." + "\n"
                                            + $"inst:{connectorDirectionID} a fpo:FlowDirection ." + "\n"
                                            + $"inst:{connectorDirectionID} fpo:hasValue '{connectorDirection}'^^xsd:string ." + "\n"
                                            + $"inst:{connectorID} ex:hasFlowDirectionVectorZ inst:{connectorDirectionVectorZID} ." + "\n"
                                            + $"inst:{connectorDirectionVectorZID} fpo:hasValue '{connectorDirectionVectorZ}'^^xsd:string ." + "\n"
                                            );

                    if (connector.Shape.ToString() == "Round")
                    {
                        connectorOuterDiameterID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                        connectorOuterDiameter = UnitUtils.ConvertFromInternalUnits(connector.Radius * 2, UnitTypeId.Meters);
                        sb.Append($"inst:{connectorID} fpo:hasOuterDiameter inst:{connectorOuterDiameterID} ." + "\n" +
                            $"inst:{connectorOuterDiameterID} fpo:hasValue '{connectorOuterDiameter}'^^xsd:double ." + "\n" +
                            $"inst:{connectorOuterDiameterID} fpo:hasUnit 'Meter'^^xsd:string  ." + "\n");

                        crossSectionalAreaID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                        crossSectionalArea = Math.PI * Math.Pow(UnitUtils.ConvertFromInternalUnits(connector.Radius, UnitTypeId.Meters), 2);
                        sb.Append($"inst:{connectorID} fpo:hasCrossSectionalArea inst:{crossSectionalAreaID} ." + "\n" +
                            $"inst:{crossSectionalAreaID} a fpo:CrossSectionalArea ." + "\n" +
                            $"inst:{crossSectionalAreaID} fpo:hasValue '{crossSectionalArea}'^^xsd:double ." + "\n" +
                            $"inst:{crossSectionalAreaID} fpo:hasUnit 'Meter'^^xsd:string  ." + "\n");
                    }

                    if (connectorDirection == "Out" && Domain.DomainHvac == connector.Domain)
                    { 
                        if(component.LookupParameter("FSC_nomFlow1") != null) { 
                            //Air side flow
                            string flowID = System.Guid.NewGuid().ToString().Replace(' ', '-'); ;
                            double flowValue = UnitUtils.ConvertFromInternalUnits(component.LookupParameter("FSC_nomFlow1").AsDouble(), UnitTypeId.LitersPerSecond);
                            sb.Append($"inst:{connectorID} fpo:hasFlowRate inst:{flowID} ." + "\n"
                         + $"inst:{flowID} a fpo:FlowRate ." + "\n"
                         + $"inst:{flowID} fpo:hasValue '{flowValue}'^^xsd:double ." + "\n"
                         + $"inst:{flowID} fpo:hasUnit 'Liters per second'^^xsd:string ." + "\n");
                        }

                        if (component.LookupParameter("FSC_nomPressureDrop1") != null)
                        {
                            //Air side pressure drop
                            string pressureDropID = System.Guid.NewGuid().ToString().Replace(' ', '-'); ;
                            double pressureDropValue = UnitUtils.ConvertFromInternalUnits(component.LookupParameter("FSC_nomPressureDrop1").AsDouble(), UnitTypeId.Pascals);
                            sb.Append($"inst:{connectorID} fpo:hasPressureDrop inst:{pressureDropID} ." + "\n"
                           + $"inst:{pressureDropID} a fpo:PressureDrop ." + "\n"
                           + $"inst:{pressureDropID} fpo:hasValue '{pressureDropValue}'^^xsd:double ." + "\n"
                           + $"inst:{pressureDropID} fpo:hasUnit 'Pascal'^^xsd:string ." + "\n");
                        }
                    }

                    if (connectorDirection == "Out" && Domain.DomainPiping == connector.Domain)
                    {
                        if(component.LookupParameter("FSC_nomFlow2") != null) { 
                            //Water side flow
                            string flowID = System.Guid.NewGuid().ToString().Replace(' ', '-'); ;
                            double flowValue = UnitUtils.ConvertFromInternalUnits(component.LookupParameter("FSC_nomFlow2").AsDouble(), UnitTypeId.LitersPerSecond);
                            sb.Append($"inst:{connectorID} fpo:hasFlowRate inst:{flowID} ." + "\n"
                          + $"inst:{flowID} a fpo:FlowRate ." + "\n"
                          + $"inst:{flowID} fpo:hasValue '{flowValue}'^^xsd:double ." + "\n"
                          + $"inst:{flowID} fpo:hasUnit 'Liters per second'^^xsd:string ." + "\n");
                        }
                        if (component.LookupParameter("FSC_nomPressureDrop2") != null)
                        {
                            //Water side pressure drop
                            string pressureDropID = System.Guid.NewGuid().ToString().Replace(' ', '-'); ;
                            double pressureDropValue = UnitUtils.ConvertFromInternalUnits(component.LookupParameter("FSC_nomPressureDrop2").AsDouble(), UnitTypeId.Pascals);
                            sb.Append($"inst:{connectorID} fpo:hasPressureDrop inst:{pressureDropID} ." + "\n"
                           + $"inst:{pressureDropID} a fpo:PressureDrop ." + "\n"
                           + $"inst:{pressureDropID} fpo:hasValue '{pressureDropValue}'^^xsd:double ." + "\n"
                           + $"inst:{pressureDropID} fpo:hasUnit 'Pascal'^^xsd:string ." + "\n");
                        }
                    }

                    //Port relationship to other ports
                    ConnectorSet joinedconnectors = connector.AllRefs;
                    if (connectorDirection == "Out")
                    {
                        foreach (Connector connectedConnector in joinedconnectors)
                        {
                            connectedConnectorID = connectedConnector.Owner.UniqueId.ToString() + "-" + connectedConnector.Id.ToString();
                            connectedComponentID = connectedConnector.Owner.UniqueId.ToString();

                            if (Domain.DomainHvac == connectedConnector.Domain && connector.DuctSystemType == DuctSystemType.SupplyAir
                            || (Domain.DomainPiping == connectedConnector.Domain && connector.PipeSystemType == PipeSystemType.SupplyHydronic))
                            {
                                connectedConnectorDirection = connectedConnector.Direction.ToString();

                                sb.Append($"inst:{connectorID} fso:suppliesFluidTo inst:{connectedConnectorID} ." + "\n"
                                    + $"inst:{connectedConnectorID} a fso:Port ." + "\n"
                                    + $"inst:{componentID} fso:feedsFluidTo inst:{connectedComponentID} ." + "\n");

                            }
                            if (Domain.DomainHvac == connectedConnector.Domain && connector.DuctSystemType == DuctSystemType.ReturnAir
                            || Domain.DomainPiping == connectedConnector.Domain && connector.PipeSystemType == PipeSystemType.ReturnHydronic)
                            {
                                connectedConnectorDirection = connectedConnector.Direction.ToString();

                                sb.Append($"inst:{connectorID} fso:returnsFluidTo inst:{connectedConnectorID} ." + "\n"
                                    + $"inst:{connectedConnectorID} a fso:Port ." + "\n"
                                    + $"inst:{componentID} fso:feedsFluidTo inst:{connectedComponentID} ." + "\n"
);

                            }



                        }
                    }
                }
            }

            return sb;
        }


        public static StringBuilder PipeConnectors(Pipe component, string componentID, StringBuilder sb)
        {
            //Port type
            ConnectorSet connectorSet = component.ConnectorManager.Connectors;

            string connectorID;
            string connectorDirectionID;
            string connectorDirection;
            double connectorOuterDiameter;
            string connectedConnectorID;
            string connectedConnectorDirection;
            string connectorOuterDiameterID;
            string connectorWidthID;
            string connectorHeightID;
            double connectorWidth;
            double connectorHeight;
            string connectedComponentID;
            string connectorDirectionVectorZID;
            string connectorDirectionVectorZ;
            string crossSectionalAreaID;
            double crossSectionalArea;

            foreach (Connector connector in connectorSet)
            {
              
                if (Domain.DomainHvac == connector.Domain || Domain.DomainPiping == connector.Domain)
                {
                    //Type
                    connectorID = componentID + "-" + connector.Id;
                    sb.Append($"inst:{componentID} fso:hasPort inst:{connectorID} ." + "\n"
                        + $"inst:{connectorID} a fso:Port ." + "\n");

                    //FlowDirection
                    connectorDirectionID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                    connectorDirection = connector.Direction.ToString();
                    connectorDirectionVectorZID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                    connectorDirectionVectorZ = connector.CoordinateSystem.BasisZ.ToString();

                    sb.Append($"inst:{connectorID} fpo:hasFlowDirection inst:{connectorDirectionID} ." + "\n"
                                            + $"inst:{connectorDirectionID} a fpo:FlowDirection ." + "\n"
                                            + $"inst:{connectorDirectionID} fpo:hasValue '{connectorDirection}'^^xsd:string ." + "\n"
                                            + $"inst:{connectorID} ex:hasFlowDirectionVectorZ inst:{connectorDirectionVectorZID} ." + "\n"
                                            + $"inst:{connectorDirectionVectorZID} fpo:hasValue '{connectorDirectionVectorZ}'^^xsd:string ." + "\n"
                                            );

                    //Port size
                    string outerdiameterID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                        double outerDiameterValue = UnitUtils.ConvertFromInternalUnits(connector.Radius*2, UnitTypeId.Meters);
                        sb.Append($"inst:{connectorID} fpo:hasOuterDiameter inst:{outerdiameterID} ." + "\n"
                         + $"inst:{outerdiameterID} a fpo:OuterDiameter ." + "\n"
                         + $"inst:{outerdiameterID} fpo:hasValue '{outerDiameterValue}'^^xsd:double ." + "\n"
                         + $"inst:{outerdiameterID} fpo:hasUnit 'Meter'^^xsd:string ." + "\n");

                    crossSectionalAreaID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                    crossSectionalArea = Math.PI * Math.Pow(UnitUtils.ConvertFromInternalUnits(connector.Radius, UnitTypeId.Meters), 2);
                    sb.Append($"inst:{connectorID} fpo:hasCrossSectionalArea inst:{crossSectionalAreaID} ." + "\n" +
                        $"inst:{crossSectionalAreaID} a fpo:CrossSectionalArea ." + "\n" +
                        $"inst:{crossSectionalAreaID} fpo:hasValue '{crossSectionalArea}'^^xsd:double ." + "\n" +
                        $"inst:{crossSectionalAreaID} fpo:hasUnit 'Meter'^^xsd:string  ." + "\n");

                    if (connector.Flow != null)
                    {
                        //Flow rate
                        string flowID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                        double flowValue = UnitUtils.ConvertFromInternalUnits(connector.Flow, UnitTypeId.LitersPerSecond);
                        sb.Append($"inst:{connectorID} fpo:hasFlowRate inst:{flowID} ." + "\n"
                         + $"inst:{flowID} a fpo:FlowRate ." + "\n"
                         + $"inst:{flowID} fpo:hasValue '{flowValue}'^^xsd:double ." + "\n"
                         + $"inst:{flowID} fpo:hasUnit 'Liters per second'^^xsd:string ." + "\n");
                    }

                    //if (connector.VelocityPressure != null)
                    //{
                    //    //Flow rate
                    //    string velocityID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                    //    double velocityValue = UnitUtils.ConvertFromInternalUnits(connector.VelocityPressure, UnitTypeId.MetersPerSecond);
                    //    sb.Append($"inst:{connectorID} fpo:velocity inst:{velocityID} ." + "\n"
                    //     + $"inst:{velocityID} a fpo:Velocity ." + "\n"
                    //     + $"inst:{velocityID} fpo:hasValue '{velocityValue}'^^xsd:double ." + "\n"
                    //     + $"inst:{velocityID} fpo:hasUnit 'Meters per second'^^xsd:string ." + "\n");
                    //}

                    //Port relationship to other ports
                    ConnectorSet joinedconnectors = connector.AllRefs;
                    if (connectorDirection == "Out")
                    {

                        //if (component.LookupParameter("Flow") != null)
                        //{
                        //    //Flow rate
                        //    string flowID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                        //    double flowValue = UnitUtils.ConvertFromInternalUnits(component.LookupParameter("Flow").AsDouble(), UnitTypeId.LitersPerSecond);
                        //    sb.Append($"inst:{connectorID} fpo:hasFlowRate inst:{flowID} ." + "\n"
                        //     + $"inst:{flowID} a fpo:FlowRate ." + "\n"
                        //     + $"inst:{flowID} fpo:hasValue '{flowValue}'^^xsd:double ." + "\n"
                        //     + $"inst:{flowID} fpo:hasUnit 'Liters per second'^^xsd:string ." + "\n");
                        //}

                        if (component.LookupParameter("Reynolds Number") != null)
                        {
                            //Reynolds number
                            string reynoldsID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                            double reynoldsValue = component.LookupParameter("Reynolds Number").AsDouble();
                            sb.Append($"inst:{connectorID} fpo:hasReynoldsNumber inst:{reynoldsID} ." + "\n"
                             + $"inst:{reynoldsID} a fpo:ReynoldsNumber ." + "\n"
                             + $"inst:{reynoldsID} fpo:hasValue '{reynoldsValue}'^^xsd:double ." + "\n");
                        }
                        if (component.LookupParameter("Friction Factor") != null)
                        {
                            //frictionFactor 
                            string frictionFactorID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                            double frictionFactorValue = component.LookupParameter("Friction Factor").AsDouble();
                            sb.Append($"inst:{connectorID} fpo:hasFrictionFactor inst:{frictionFactorID} ." + "\n"
                             + $"inst:{frictionFactorID} a fpo:FrictionFactor ." + "\n"
                             + $"inst:{frictionFactorID} fpo:hasValue '{frictionFactorValue}'^^xsd:double ." + "\n");
                        }

                        //if (component.LookupParameter("Velocity") != null)
                        //{
                        //    //Flow Velocity
                        //    string velocityID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                        //    double velocityValue = UnitUtils.ConvertFromInternalUnits(component.LookupParameter("Velocity").AsDouble(), UnitTypeId.MetersPerSecond);
                        //    sb.Append($"inst:{connectorID} fpo:velocity inst:{velocityID} ." + "\n"
                        //     + $"inst:{velocityID} a fpo:Velocity ." + "\n"
                        //     + $"inst:{velocityID} fpo:hasValue '{velocityValue}'^^xsd:double ." + "\n"
                        //     + $"inst:{velocityID} fpo:hasUnit 'Meters per second'^^xsd:string ." + "\n");
                        //}

                        //if (component.LookupParameter("Pressure Drop") != null)
                        //{
                        //    //Pressure Drop
                        //    string pressureDropID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                        //    double pressureDropValue = UnitUtils.ConvertFromInternalUnits(component.LookupParameter("Pressure Drop").AsDouble(), UnitTypeId.Pascals);

                        //    if (component.LookupParameter("Velocity Pressure") != null)
                        //    {
                        //        double VelocityPressureDropValue = UnitUtils.ConvertFromInternalUnits(component.LookupParameter("Velocity Pressure").AsDouble(), UnitTypeId.Pascals);
                        //        pressureDropValue += VelocityPressureDropValue;
                        //    }

                        //    sb.Append($"inst:{connectorID} fpo:hasPressureDrop inst:{pressureDropID} ." + "\n"
                        //     + $"inst:{pressureDropID} a fpo:PressureDrop ." + "\n"
                        //     + $"inst:{pressureDropID} fpo:hasValue '{pressureDropValue}'^^xsd:double ." + "\n"
                        //     + $"inst:{pressureDropID} fpo:hasUnit 'Pascal'^^xsd:string ." + "\n");
                        //};

                        if (component.LookupParameter("Flow State") != null)
                        {
                            //Flow State
                            string flowStateID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                            string flowStateValue = component.LookupParameter("Flow State").AsValueString();
                            sb.Append($"inst:{connectorID} fpo:hasFlowState inst:{flowStateID} ." + "\n"
                             + $"inst:{flowStateID} a fpo:FlowState ." + "\n"
                             + $"inst:{flowStateID} fpo:hasValue '{flowStateValue}'^^xsd:string ." + "\n"
                             + $"inst:{flowStateID} fpo:hasUnit 'Pascal'^^xsd:string ." + "\n");
                        }


                        foreach (Connector connectedConnector in joinedconnectors)
                        {

                            connectedConnectorID = connectedConnector.Owner.UniqueId.ToString() + "-" + connectedConnector.Id.ToString();
                            connectedComponentID = connectedConnector.Owner.UniqueId.ToString();

                            if (connector.Owner.UniqueId != connectedConnector.Owner.UniqueId ) { 
                            if (Domain.DomainHvac == connectedConnector.Domain && connector.DuctSystemType == DuctSystemType.SupplyAir
                            || (Domain.DomainPiping == connectedConnector.Domain && connector.PipeSystemType == PipeSystemType.SupplyHydronic))
                            {
                                connectedConnectorDirection = connectedConnector.Direction.ToString();

                                sb.Append($"inst:{connectorID} fso:suppliesFluidTo inst:{connectedConnectorID} ." + "\n" 
                                    + $"inst:{connectedConnectorID} a fso:Port ." + "\n"
                                    + $"inst:{componentID} fso:feedsFluidTo inst:{connectedComponentID} ." + "\n"
                                    );

                            }
                            if (Domain.DomainHvac == connectedConnector.Domain && connector.DuctSystemType == DuctSystemType.ReturnAir
                            || Domain.DomainPiping == connectedConnector.Domain && connector.PipeSystemType == PipeSystemType.ReturnHydronic)
                            {
                                connectedConnectorDirection = connectedConnector.Direction.ToString();

                                sb.Append($"inst:{connectorID} fso:returnsFluidTo inst:{connectedConnectorID} ." + "\n"
                                    + $"inst:{connectedConnectorID} a fso:Port ." + "\n" 
                                    + $"inst:{componentID} fso:feedsFluidTo inst:{connectedComponentID} ." + "\n"
);

                            }
                            }


                        }
                    }
                }
            }

            return sb;
        }


        public static StringBuilder DuctConnectors(Duct component, string componentID, StringBuilder sb)
        {
            //Port type
            ConnectorSet connectorSet = component.ConnectorManager.Connectors;

            string connectorID;
            string connectorDirectionID;
            string connectorDirection;
            double connectorOuterDiameter;
            string connectedConnectorID;
            string connectedConnectorDirection;
            string connectorOuterDiameterID;
            string connectorWidthID;
            string connectorHeightID;
            double connectorWidth;
            double connectorHeight;
            string connectedComponentID;
            string connectorDirectionVectorZID;
            string connectorDirectionVectorZ;
            string crossSectionalAreaID;
            double crossSectionalArea;

            foreach (Connector connector in connectorSet)
            {
                
                if (Domain.DomainHvac == connector.Domain || Domain.DomainPiping == connector.Domain)
                {
                    //Type
                    connectorID = componentID + "-" + connector.Id;
                    sb.Append($"inst:{componentID} fso:hasPort inst:{connectorID} ." + "\n"
                        + $"inst:{connectorID} a fso:Port ." + "\n");


                    //FlowDirection
                    connectorDirectionID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                    connectorDirection = connector.Direction.ToString();
                    connectorDirectionVectorZID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                    connectorDirectionVectorZ = connector.CoordinateSystem.BasisZ.ToString();

                    sb.Append($"inst:{connectorID} fpo:hasFlowDirection inst:{connectorDirectionID} ." + "\n"
                                            + $"inst:{connectorDirectionID} a fpo:FlowDirection ." + "\n"
                                            + $"inst:{connectorDirectionID} fpo:hasValue '{connectorDirection}'^^xsd:string ." + "\n"
                                            + $"inst:{connectorID} ex:hasFlowDirectionVectorZ inst:{connectorDirectionVectorZID} ." + "\n"
                                            + $"inst:{connectorDirectionVectorZID} fpo:hasValue '{connectorDirectionVectorZ}'^^xsd:string ." + "\n"
                                            );

                    //Size
                    if (connector.Shape.ToString() == "Round")
                    {
                        connectorOuterDiameterID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                        connectorOuterDiameter = UnitUtils.ConvertFromInternalUnits(connector.Radius * 2, UnitTypeId.Meters);
                        sb.Append($"inst:{connectorID} fpo:hasOuterDiameter inst:{connectorOuterDiameterID} ." + "\n" +
                            $"inst:{connectorOuterDiameterID} a fpo:OuterDiameter ." + "\n" +
                            $"inst:{connectorOuterDiameterID} fpo:hasValue '{connectorOuterDiameter}'^^xsd:double ." + "\n" +
                            $"inst:{connectorOuterDiameterID} fpo:hasUnit 'Meter'^^xsd:string  ." + "\n");

                        crossSectionalAreaID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                        crossSectionalArea = Math.PI * Math.Pow(UnitUtils.ConvertFromInternalUnits(connector.Radius, UnitTypeId.Meters),2);
                        sb.Append($"inst:{connectorID} fpo:hasCrossSectionalArea inst:{crossSectionalAreaID} ." + "\n" +
                            $"inst:{crossSectionalAreaID} a fpo:CrossSectionalArea ." + "\n" +
                            $"inst:{crossSectionalAreaID} fpo:hasValue '{crossSectionalArea}'^^xsd:double ." + "\n" +
                            $"inst:{crossSectionalAreaID} fpo:hasUnit 'Meter'^^xsd:string  ." + "\n");

                    }

                    if (connector.Shape.ToString() == "Rectangular")
                    {
                        connectorWidthID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                        connectorHeightID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                        connectorHeight = UnitUtils.ConvertFromInternalUnits(connector.Height, UnitTypeId.Meters);
                        connectorWidth = UnitUtils.ConvertFromInternalUnits(connector.Width, UnitTypeId.Meters);
                        sb.Append($"inst:{connectorID} fpo:hasHeight inst:{connectorHeightID} ." + "\n" +
                            $"inst:{connectorHeightID} a fpo:Heigth ." + "\n" +
                            $"inst:{connectorHeightID} fpo:hasValue '{connectorHeight}'^^xsd:double ." + "\n" +
                            $"inst:{connectorHeightID} fpo:hasUnit 'Meter'^^xsd:string  ." + "\n" +
                            $"inst:{connectorID} fpo:hasWidth inst:{connectorWidthID} ." + "\n" +
                            $"inst:{connectorWidthID} a fpo:Width ." + "\n" +
                            $"inst:{connectorWidthID} fpo:hasValue '{connectorWidth}'^^xsd:double ." + "\n" +
                            $"inst:{connectorWidthID} fpo:hasUnit 'Meter'^^xsd:string  ." + "\n");

                        crossSectionalAreaID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                        crossSectionalArea = connectorHeight * connectorWidth;
                        sb.Append($"inst:{connectorID} fpo:hasCrossSectionalArea inst:{crossSectionalAreaID} ." + "\n" +
                            $"inst:{crossSectionalAreaID} a fpo:CrossSectionalArea ." + "\n" +
                            $"inst:{crossSectionalAreaID} fpo:hasValue '{crossSectionalArea}'^^xsd:double ." + "\n" +
                            $"inst:{crossSectionalAreaID} fpo:hasUnit 'Meter'^^xsd:string  ." + "\n");

                    }

                    if (connector.Flow != null)
                    {
                        //Flow rate
                        string flowID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                        double flowValue = UnitUtils.ConvertFromInternalUnits(connector.Flow, UnitTypeId.LitersPerSecond);
                        sb.Append($"inst:{connectorID} fpo:hasFlowRate inst:{flowID} ." + "\n"
                         + $"inst:{flowID} a fpo:FlowRate ." + "\n"
                         + $"inst:{flowID} fpo:hasValue '{flowValue}'^^xsd:double ." + "\n"
                         + $"inst:{flowID} fpo:hasUnit 'Liters per second'^^xsd:string ." + "\n");
                    }

                    //if (connector.VelocityPressure != null)
                    //{
                    //    //Flow rate
                    //    string velocityID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                    //    double velocityValue = UnitUtils.ConvertFromInternalUnits(connector.VelocityPressure, UnitTypeId.MetersPerSecond);
                    //    sb.Append($"inst:{connectorID} fpo:velocity inst:{velocityID} ." + "\n"
                    //     + $"inst:{velocityID} a fpo:Velocity ." + "\n"
                    //     + $"inst:{velocityID} fpo:hasValue '{velocityValue}'^^xsd:double ." + "\n"
                    //     + $"inst:{velocityID} fpo:hasUnit 'Meters per second'^^xsd:string ." + "\n");
                    //}

                    //Port relationship to other ports
                    ConnectorSet joinedconnectors = connector.AllRefs;
                    if (connectorDirection == "Out")
                    {

                        //if (component.LookupParameter("Flow") != null)
                        //{
                        //    //Flow rate
                        //    string flowID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                        //    double flowValue = UnitUtils.ConvertFromInternalUnits(component.LookupParameter("Flow").AsDouble(), UnitTypeId.LitersPerSecond);
                        //    sb.Append($"inst:{connectorID} fpo:hasFlowRate inst:{flowID} ." + "\n"
                        //     + $"inst:{flowID} a fpo:FlowRate ." + "\n"
                        //     + $"inst:{flowID} fpo:hasValue '{flowValue}'^^xsd:double ." + "\n"
                        //     + $"inst:{flowID} fpo:hasUnit 'Liters per second'^^xsd:string ." + "\n");
                        //}

                        if (component.LookupParameter("Reynolds number") != null)
                        {
                            //Reynolds number
                            string reynoldsID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                            double reynoldsValue = component.LookupParameter("Reynolds number").AsDouble();
                            sb.Append($"inst:{connectorID} fpo:hasReynoldsNumber inst:{reynoldsID} ." + "\n"
                             + $"inst:{reynoldsID} a fpo:ReynoldsNumber ." + "\n"
                             + $"inst:{reynoldsID} fpo:hasValue '{reynoldsValue}'^^xsd:double ." + "\n");
                        }


                        ////Flow Velocity
                        //string velocityID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                        //double velocityValue = UnitUtils.ConvertFromInternalUnits(component.LookupParameter("Velocity").AsDouble(), UnitTypeId.MetersPerSecond);
                        //sb.Append($"inst:{connectorID} fpo:velocity inst:{velocityID} ." + "\n"
                        // + $"inst:{velocityID} a fpo:Velocity ." + "\n"
                        // + $"inst:{velocityID} fpo:hasValue '{velocityValue}'^^xsd:double ." + "\n"
                        // + $"inst:{velocityID} fpo:hasUnit 'Meters per second'^^xsd:string ." + "\n");

                        //if (component.LookupParameter("Pressure Drop") != null)
                        //{
                        //    //Pressure Drop
                        //    string pressureDropID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                        //    double pressureDropValue = UnitUtils.ConvertFromInternalUnits(component.LookupParameter("Pressure Drop").AsDouble(), UnitTypeId.Pascals);
                        //    if (component.LookupParameter("Velocity Pressure") != null)
                        //    {
                        //        double VelocityPressureDropValue = UnitUtils.ConvertFromInternalUnits(component.LookupParameter("Velocity Pressure").AsDouble(), UnitTypeId.Pascals);
                        //        pressureDropValue += VelocityPressureDropValue;
                        //    }
                        //        sb.Append($"inst:{connectorID} fpo:hasPressureDrop inst:{pressureDropID} ." + "\n"
                        //     + $"inst:{pressureDropID} a fpo:PressureDrop ." + "\n"
                        //     + $"inst:{pressureDropID} fpo:hasValue '{pressureDropValue}'^^xsd:double ." + "\n"
                        //     + $"inst:{pressureDropID} fpo:hasUnit 'Pascal'^^xsd:string ." + "\n");
                        //};

                        if (component.LookupParameter("Flow State") != null)
                        {
                            //Flow State
                            string flowStateID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                            string flowStateValue = component.LookupParameter("Flow State").AsValueString();
                            sb.Append($"inst:{connectorID} fpo:hasFlowState inst:{flowStateID} ." + "\n"
                             + $"inst:{flowStateID} a fpo:FlowState ." + "\n"
                             + $"inst:{flowStateID} fpo:hasValue '{flowStateValue}'^^xsd:string ." + "\n"
                             + $"inst:{flowStateID} fpo:hasUnit 'Pascal'^^xsd:string ." + "\n");
                        }

                        foreach (Connector connectedConnector in joinedconnectors)
                        {
                            connectedConnectorID = connectedConnector.Owner.UniqueId.ToString() + "-" + connectedConnector.Id.ToString();
                            connectedComponentID = connectedConnector.Owner.UniqueId.ToString();

                            if (connector.Owner.UniqueId != connectedConnector.Owner.UniqueId)
                            {
                                if (Domain.DomainHvac == connectedConnector.Domain && connector.DuctSystemType == DuctSystemType.SupplyAir
                                || (Domain.DomainPiping == connectedConnector.Domain && connector.PipeSystemType == PipeSystemType.SupplyHydronic))
                                {
                                    connectedConnectorDirection = connectedConnector.Direction.ToString();

                                    sb.Append($"inst:{connectorID} fso:suppliesFluidTo inst:{connectedConnectorID} ." + "\n"
                                        + $"inst:{connectedConnectorID} a fso:Port ." + "\n"
                                        + $"inst:{componentID} fso:feedsFluidTo inst:{connectedComponentID} ." + "\n"
                                        );

                                }
                                if (Domain.DomainHvac == connectedConnector.Domain && connector.DuctSystemType == DuctSystemType.ReturnAir
                                || Domain.DomainPiping == connectedConnector.Domain && connector.PipeSystemType == PipeSystemType.ReturnHydronic)
                                {
                                    connectedConnectorDirection = connectedConnector.Direction.ToString();

                                    sb.Append($"inst:{connectorID} fso:returnsFluidTo inst:{connectedConnectorID} ." + "\n"
                                        + $"inst:{connectedConnectorID} a fso:Port ." + "\n"
                                        + $"inst:{componentID} fso:feedsFluidTo inst:{connectedComponentID} ." + "\n"
    );

                                }
                            }



                        }
                    }
                }
            }

            return sb;
        }

       



    }
}
