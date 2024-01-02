import React from 'react'
import {useDataUpdate} from 'hookui-framework'
import $Panel from './panel'
  
const DemandSection2 = ({title, value, factors }) => {
  return (
    <div class="infoview-panel-section_RXJ" style={{width: '95%', paddingTop: '3rem', paddingBottom: '3rem'}}>
	  {/* title */}
	  <div class="labels_L7Q row_S2v uppercase_RJI">
	    <div class="left_Lgw row_S2v">{title}</div>
		{ value >= 0 && ( <div class="right_k30 row_S2v">{Math.round(value*100)}</div> ) }
	  </div>
	  <div class="space_uKL" style={{height: '3rem'}}></div>
	  {/* factors */}
      {factors.map((item, index) => (
		<div key={index} class="labels_L7Q row_S2v small_ExK" style={{marginTop: '1rem'}}>
		  <div class="left_Lgw row_S2v">{item["factor"]}</div>
		  <div class="right_k30 row_S2v">
		  {item["weight"] < 0 ?
		  <div class="negative_YWY">{item["weight"]}</div> :
		  <div class="positive_zrK">{item["weight"]}</div>}
		  </div>  
		</div>
      ))}
	</div>
  );  	
};

const RowWithTwoColumns = ({left, right}) => {
	return (
	<div class="labels_L7Q row_S2v">
		<div class="row_S2v" style={{width: '60%'}}>{left}</div>
		<div class="row_S2v" style={{width: '40%', justifyContent: 'center'}}>{right}</div>
	</div>
	);
};

const RowWithThreeColumns = ({left, leftSmall, right1, right2}) => {
	return (
	<div class="labels_L7Q row_S2v">
		<div class="row_S2v" style={{width: '60%', flexDirection: 'column'}}>
			<p>{left}</p>
			<p style={{fontSize: '80%'}}>{leftSmall}</p>
		</div>
		<div class="row_S2v" style={{width: '20%', justifyContent: 'center'}}>{right1}</div>
		<div class="row_S2v" style={{width: '20%', justifyContent: 'center'}}>{right2}</div>
	</div>
	);
};

// simple horizontal line
const DataDivider = () => {
	return (
	<div style={{display: 'flex', height: '4rem', flexDirection: 'column', justifyContent: 'center'}}>
		<div style={{borderBottom: '1px solid gray'}}></div>
	</div>
	);
};

const ColumnCommercialData = ({ data }) => {
	return (
	<div style={{width: '70%', boxSizing: 'border-box', border: '1px solid gray'}}>
	
		<RowWithThreeColumns left="EMPTY BUILDINGS" right1={data[0]} />
		<RowWithThreeColumns left="PROPERTYLESS COMPANIES" right1={data[1]} />
		
		<DataDivider />
		
		<RowWithThreeColumns left="AVERAGE TAX RATE" leftSmall="10% is the neutral rate" right1={`${data[2]/10} %`} />
		
		<DataDivider />

		<div class="labels_L7Q row_S2v">
			<div class="row_S2v" style={{width: '60%'}} />
			<div class="row_S2v" style={{width: '20%', justifyContent: 'center'}}>Standard</div>
			<div class="row_S2v" style={{width: '20%', justifyContent: 'center'}}>Leisure</div>
		</div>
		
		<RowWithThreeColumns left="SERVICE UTILIZATION" leftSmall="30% is the neutral ratio" right1={`${data[3]} %`} right2={`${data[4]} %`} />
		<RowWithThreeColumns left="SALES CAPACITY" leftSmall="100% when capacity = consumption" right1={`${data[5]} %`} right2={`${data[6]} %`} />
		
		<DataDivider />
		
		<RowWithThreeColumns left="Employee capacity ratio" leftSmall="75% is the neutral ratio" right1={`${data[7]/10} %`} />
		
		<DataDivider />
		
		<div style={{display: 'flex'}}>
			<div style={{width: '60%', height: '2.2em', display: 'flex', alignItems: 'center'}}>
				AVAILABLE WORKFORCE
			</div>
			<div style={{width: '40%'}}>
				<RowWithTwoColumns left="Educated" right={data[8]} />
				<RowWithTwoColumns left="Uneducated" right={data[9]} />
			</div>
		</div>
	</div>
	);
};

const ColumnExcludedResources = ({ resources }) => {
	return (
	<div style={{width: '30%', boxSizing: 'border-box', border: '1px solid gray'}}>
		<div class="row_S2v">No demand for:</div>
        <ul>
          {resources.map((item, index) => (
            <li key={index}>
				<div class="row_S2v small_ExK">{item}</div>
			</li>
          ))}
        </ul>
	</div>
	);
};

const $Commercial = ({ react }) => {
	
	// commercial data
	const [commercialData, setCommercialData] = react.useState([])
	useDataUpdate(react, 'cityInfo.ilCommercial', setCommercialData)
	
	// excluded resources
	const [excludedResources, setExcludedResources] = react.useState([])
	useDataUpdate(react, 'cityInfo.ilCommercialExRes', setExcludedResources)

	const onClose = () => {
		const data = { type: "toggle_visibility", id: 'infoloom.commercial' };
		const event = new CustomEvent('hookui', { detail: data });
		window.dispatchEvent(event);
	};

	return <$Panel react={react} title="Commercial Data" onClose={onClose} initialSize={{ width: window.innerWidth * 0.25, height: window.innerHeight * 0.26 }} initialPosition={{ top: window.innerHeight * 0.05, left: window.innerWidth * 0.005 }}>	
		{commercialData.length === 0 ? (
			<p>Waiting...</p>
		) : (
		<div style={{display: 'flex'}}>
			<ColumnCommercialData data={commercialData} />
			<ColumnExcludedResources resources={excludedResources} />
		</div>
		)}
	</$Panel>
};

// Registering the panel with HookUI so it shows up in the menu
window._$hookui.registerPanel({
	id: "infoloom.commercial",
	name: "InfoLoom: Commercial",
	icon: "Media/Game/Icons/ZoneCommercial.svg",
	component: $Commercial
});
