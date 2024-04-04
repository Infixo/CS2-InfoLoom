// Credits: Captain-Of-Coit
// The function is from 'hookui-framework'
// https://github.com/Captain-Of-Coit/hookui-framework/blob/master/src/helpers/use-data-update.js

const useDataUpdate = (react, event, onUpdate, deps) => {
    return react.useEffect(() => {
        const updateEvent = event + ".update"
        const subscribeEvent = event + ".subscribe"
        const unsubscribeEvent = event + ".unsubscribe"
    
        var sub = engine.on(updateEvent, (data) => {
            onUpdate && onUpdate(data)
        })

        engine.trigger(subscribeEvent)
        return () => {
            engine.trigger(unsubscribeEvent)
            sub.clear()
        };
    }, deps || [])
}

export default useDataUpdate