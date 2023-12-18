import React from 'react'
import {useDataUpdate} from 'hookui-framework'
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

const TableExample = () => {
	
  const rows = Array.from({ length: 6 }, (_, rowIndex) => (
    <tr key={rowIndex}>
      {Array.from({ length: 7 }, (_, colIndex) => (
        <td key={colIndex} style={tableStyles.tableCell}>
          <div class="row_S2v">Row {rowIndex + 1}, Col {colIndex + 1}</div>
        </td>
      ))}
    </tr>
  ));
  return (
    <div style={tableStyles.tableContainer}>
      <table style={tableStyles.customTable}>
        {rows}
      </table>
    </div>
  );
};

const WorkforceLevel = ({levelColor, levelName, values}) => {
  console.log(levelColor, levelName, values);
  return (
		<div class="legend_fqG">
		  <div class="color-legend_Bzi">
		    <div class="symbol_aAH" style={{backgroundColor: levelColor }}></div>
	        <div>{levelName}</div>
		  </div>
		  num1 num2 num3 num4
		</div>  
  );
};

const $Workforce = ({react}) => {
	
    //const [oldestCim, setOldestCim] = react.useState(0)
    //useDataUpdate(react, 'populationInfo.oldest_citizen', setOldestCim)
	
    // 0 - num citizens in the city 0 = 1+2+3
    // 1 - num locals
    // 2 - num tourists
    // 3 - num commuters
    // 4 - num students (in locals) 4 <= 1
    // 5 - num workers (in locals) 5 <= 1
    // 6 - oldest cim
	const [jobs, setJobs] = react.useState([]);
	useDataUpdate(react, 'workplaces.ilWorkplaces', setJobs);

	//const [details, setDetails] = react.useState([])
	//useDataUpdate(react, 'populationInfo.structureDetails', setDetails)


	// TEST
	//const numbers = Array.from({ length: 20 }, (_, index) => index + 1)

	//const lines = [];
	//for (let i = 0; i < numbers.length; i++) {
		//lines.push(<HorizontalLine key={i} length={numbers[i]*50} />);
	//}
	//paragraphs.push( <HorizontalLine key={i} length={totals[i]/100} /> );
	
	const onClose = () => {
		const data = { type: "toggle_visibility", id: 'infoloom.workforce' };
		const event = new CustomEvent('hookui', { detail: data });
		window.dispatchEvent(event);
	};

    const headers = ['Residential Low','Residential Medium','Residential High','Commercial','Industrial','Storage','Office'];
	
	return <$Panel react={react} title="Workforce" onClose={onClose} initialSize={{ width: window.innerWidth * 0.5, height: window.innerHeight * 0.5 }} initialPosition={{ top: window.innerHeight * 0.05, left: window.innerWidth * 0.005 }}>
	  <WorkforceLevel                      levelName='Education' values={headers} />
	  <WorkforceLevel levelColor='#808080' levelName='Uneducated' values={jobs[0]} />
	  <WorkforceLevel levelColor='#B09868' levelName='Poorly Educated' values={jobs[1]} />
	  <WorkforceLevel levelColor='#368A2E' levelName='Educated' values={jobs[2]} />
	  <WorkforceLevel levelColor='#B981C0' levelName='Well Educated' values={jobs[3]} />
	  <WorkforceLevel levelColor='#5796D1' levelName='Highly Educated' values={jobs[4]} />
	  <WorkforceLevel                      levelName='TOTAL' values={jobs[5]} />
	</$Panel>
}

// Registering the panel with HookUI so it shows up in the menu
window._$hookui.registerPanel({
    id: "infoloom.workforce",
    name: "Workforce",
	icon: "Media/Game/Icons/Workers.svg",
    component: $Workforce
})

