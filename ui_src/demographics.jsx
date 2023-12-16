import React from 'react'
import {useDataUpdate, $Panel} from 'hookui-framework'

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


const $Demographics = ({react}) => {
	
    const [oldestCim, setOldestCim] = react.useState(0)
    useDataUpdate(react, 'populationInfo.oldest_citizen', setOldestCim)
	
    // 0 - num citizens in the city 0 = 1+2+3
    // 1 - num locals
    // 2 - num tourists
    // 3 - num commuters
    // 4 - num students (in locals) 4 <= 1
    // 5 - num workers (in locals) 5 <= 1
    // 6 - oldest cim
	const [totals, setTotals] = react.useState([])
	useDataUpdate(react, 'populationInfo.structureTotals', setTotals)

	const [details, setDetails] = react.useState([])
	useDataUpdate(react, 'populationInfo.structureDetails', setDetails)


	// TEST
	const numbers = Array.from({ length: 20 }, (_, index) => index + 1)

	const lines = [];
	for (let i = 0; i < numbers.length; i++) {
		lines.push(<HorizontalLine key={i} length={numbers[i]*50} />);
	}
	//paragraphs.push( <HorizontalLine key={i} length={totals[i]/100} /> );

    return <$Panel react={react} title="Demographics">
		<div>
			<AlignedParagraph left="All Citizens" right={totals[0]} />
			<AlignedParagraph left="- Locals" right={totals[1]} />
			<AlignedParagraph left="- Tourists" right={totals[2]} />
			<AlignedParagraph left="- Commuters" right={totals[3]} />
			<AlignedParagraph left="Students" right={totals[4]} />
			<AlignedParagraph left="Workers" right={totals[5]} />
			<AlignedParagraph left="Oldest citizen" right={totals[6]} />
		</div>
		<div>
		{(() => {
			const paragraphs = [];
			details.forEach( info => {
				paragraphs.push( <p style={{ fontSize: '75%' }} key={info["age"]}> {info["age"]}  {info["total"]}  {info["school1"]}  {info["school2"]}  {info["school3"]}  {info["school4"]}  {info["work"]}  {info["other"]}</p> );
			});
			return paragraphs;
		})()}
		</div>
		
	</$Panel>
}

// Registering the panel with HookUI so it shows up in the menu
window._$hookui.registerPanel({
    id: "infixo.demographics",
    name: "Demographics",
    icon: "Media/Game/Icons/Population.svg",
    component: $Demographics
})

