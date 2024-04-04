import React from 'react'
import useDataUpdate from './use-data-update.js'
import $Panel from './panel'

const AlignedParagraph = ({ left, right }) => {
  const containerStyle = {
    display: 'flex',
    justifyContent: 'space-between',
    textAlign: 'justify',
	marginBottom: '0.25em', // Add some spacing between the <p> tags
  };
  const leftTextStyle = {
    width: '40%',
	marginLeft: '10%', // Start 10% from the left edge
  };
  const rightTextStyle = {
    width: '40%',
	marginRight: '10%', // Start 10% from the right edge
    textAlign: 'right',
  };	
  return (
    <p style={containerStyle}>
      <span style={leftTextStyle}>{left}</span>
      <span style={rightTextStyle}>{right}</span>
    </p>
  );
};

const HorizontalLine = ({ length }) => {
  const lineStyle = {
    width: `${length}px`,
    borderBottom: '5px solid white', // Adjust the border style as needed
    margin: '1px 0', // Adjust the margin as needed
  };
  return <div style={lineStyle}></div>;
};

// Define styles outside the component
const tableStyles = {
  tableContainer: {
    margin: '20px',
  },
  customTable: {
    borderCollapse: 'collapse',
    //width: '100%',
  },
  tableCell: {
    border: '1px solid #ddd',
    padding: '8px',
    textAlign: 'center',
	width: '10%',
  },
  tableHeader: {
    backgroundColor: '#f2f2f2',
  },
};


const WorkforceLevel = ({levelColor, levelName, levelValues, total}) => {
  //console.log(levelColor); console.log(levelName); console.log(levelValues);
  // <div class="legend_fqG" style={{justifyContent: 'spaceEvenly'}}>
  const percent = ( total > 0 ? (100*levelValues.total/total).toFixed(1)+"%" : "");
  const unemployment = ( levelValues.total > 0 ? (100*levelValues.unemployed/levelValues.total).toFixed(1)+"%" : "");
  return (
    <div class="labels_L7Q row_S2v" style={{width: '99%', paddingTop: '1rem', paddingBottom: '1rem'}}>
		<div style={{width: '1%'}}></div>
		<div style={{ display: 'flex', alignItems: 'center', width: '22%' }}>
			<div class="symbol_aAH" style={{backgroundColor: levelColor, width: '1.2em' }}></div>
			<div>{levelName}</div>
		</div>
		  <div class="row_S2v"           style={{width: '11%', justifyContent: 'center'}}>{levelValues["total"]}</div>
		  <div class="row_S2v"           style={{width: '8%', justifyContent: 'center'}}>{percent}</div>
		  <div class="row_S2v"           style={{width: '11%', justifyContent: 'center'}}>{levelValues["worker"]}</div>
		  <div class="row_S2v"           style={{width: '12%', justifyContent: 'center'}}>{levelValues["unemployed"]}</div>
		  <div class="row_S2v small_ExK" style={{width: '8%', justifyContent: 'center'}}>{unemployment}</div>
		  <div class="row_S2v small_ExK" style={{width: '9%', justifyContent: 'center'}}>{levelValues["under"]}</div>
		  <div class="row_S2v small_ExK" style={{width: '9%', justifyContent: 'center'}}>{levelValues["outside"]}</div>
		  <div class="row_S2v small_ExK" style={{width: '9%', justifyContent: 'center'}}>{levelValues["homeless"]}</div>
	</div>
  );
};
	
const $Workforce = ({react}) => {
	
	const [workforce, setWorkforce] = react.useState([])
	useDataUpdate(react, 'populationInfo.ilWorkforce', setWorkforce)
	
	const onClose = () => {
		// HookUI
		//const data = { type: "toggle_visibility", id: 'infoloom.workforce' };
		//const event = new CustomEvent('hookui', { detail: data });
		//window.dispatchEvent(event);
		// Gooee
        engine.trigger("infoloom.infoloom.OnToggleVisibleWorkforce", "Workforce");
        engine.trigger("audio.playSound", "close-panel", 1);
	};

    const headers = {
		total: 'Total',
		worker: 'Workers',
		unemployed: 'Unemployed',
		homeless: 'Homeless',
		employable: 'Employable',
		under: 'Under',
		outside: 'Outside',
	};
	
	//if (workplaces.length !== 0) {
//		console.log("workplaces=", workplaces);
		//console.log(workplaces[0]); console.log(workplaces[1]);
	//} else {
//		console.log("workplaces has 0 elements");
//	}

	return <$Panel react={react} title="Workforce Structure" onClose={onClose} initialSize={{ width: window.innerWidth * 0.33, height: window.innerHeight * 0.20 }} initialPosition={{ top: window.innerHeight * 0.05, left: window.innerWidth * 0.005 }}>
		{workforce.length === 0 ? (
			<p>Waiting...</p>
		) : (
		<div>
	  <div style={{height: '10rem'}}></div>
	  <WorkforceLevel                      levelName='Education' levelValues={headers} total={0} />
	  <div style={{height: '5rem'}}></div>
	  <WorkforceLevel levelColor='#808080' levelName='Uneducated' levelValues={workforce[0]} total={workforce[5].total}/>
	  <WorkforceLevel levelColor='#B09868' levelName='Poorly Educated' levelValues={workforce[1]} total={workforce[5].total}/>
	  <WorkforceLevel levelColor='#368A2E' levelName='Educated' levelValues={workforce[2]} total={workforce[5].total}/>
	  <WorkforceLevel levelColor='#B981C0' levelName='Well Educated' levelValues={workforce[3]} total={workforce[5].total}/>
	  <WorkforceLevel levelColor='#5796D1' levelName='Highly Educated' levelValues={workforce[4]} total={workforce[5].total}/>
	  <div style={{height: '5rem'}}></div>
	  <WorkforceLevel                      levelName='TOTAL' levelValues={workforce[5]} total={0} />
	  </div>
		)}
	</$Panel>

}

export default $Workforce

// Registering the panel with HookUI so it shows up in the menu
/*
window._$hookui.registerPanel({
    id: "infoloom.workforce",
    name: "InfoLoom: Workforce",
	icon: "Media/Game/Icons/Workers.svg",
    component: $Workforce
})
*/