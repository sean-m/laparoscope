// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


let ObjectTable = {
    props: ['record', 'title', 'expand', 'list'],
    data() {
        return {};
    },
    methods: {
        expandOnCase(input) {
            return input.replace(/([a-z])([A-Z])/g, '$1 $2')
        }
    },
    template: `<div>
    <table class="table table-responsive-sm">
        <thead v-if="title">
            <tr><td><strong>{{ title }}</strong></td><td></td></tr>
        </thead>
        <tbody>
            <tr v-for="(value, key, i) in record">
                <td>{{ expandOnCase(key) }}</td>
                <td v-if="expand && expand.includes(key)">
                    <ObjectTable :record="value" :list="list"></ObjectTable>
                </td>
                <td v-else-if="list && list.includes(key)">
                    <ul class="no-bullets">
                        <li v-for="(entry) in value.sort()">{{ entry }}</li>
                    </ul>
                </td>
                <td v-else>{{ value }}</td>
            </tr>
        </tbody>
    </table>
</div>`
}

let SearchTable = {
    props: ['records', 'columns'],
    data() {
        return {};
    },
    methods : {        
        expandOnCase(input) {
            return input.replace(/([a-z])([A-Z])/g, '$1 $2')
        }
    },
    template: `
    <table class="table">
    <tr>
        <th v-for="(value, key, i) in records[0]">{{ expandOnCase(key) }}</td>
    </tr>
    <tr v-for="value of records">
        <td v-for="(val, key, i) in value">{{ val }}</td>
    </tr>
</table>
    `
}


class FilterBuilder {
    startMatch = []
    endMatch = []
    eqMatch = []
    containsMatch = []
    select = []

    constructor(operator='or') {
        this.operator = operator;
    }

    addStartMatch(property) {
        if (property == null || property == undefined) {
            return this;
        }

        if (Array.isArray(property)) {
            this.startMatch = this.startMatch.concat(property)
        } else {
            this.startMatch.push(property)
        }
        return this;
    }

    addEqMatch(property) {
        if (property == null || property == undefined) {
            return this;
        }

        if (Array.isArray(property)) {
            this.eqMatch = this.eqMatch.concat(property)
        } else {
            this.eqMatch.push(property)
        }
        return this;
    }

    addEndMatch(property) {
        if (property == null || property == undefined) {
            return this;
        }

        if (Array.isArray(property)) {
            this.endMatch = this.endMatch.concat(property)
        } else {
            this.endMatch.push(property)
        }
        return this;
    }

    addContainsMatch(property) {
        if (property == null || property == undefined) {
            return this;
        }

        if (Array.isArray(property)) {
            this.containsMatch = this.containsMatch.concat(property)
        } else {
            this.containsMatch.push(property)
        }
        return this;
    }

    addPropertySelection(property) {
        if (property == null || property == undefined) {
            return this;
        }

        if (Array.isArray(property)) {
            this.select = this.select.concat(property)
        } else {
            this.select.push(property)
        }
        return this;
    }

    setOperator(operator) {
        this.operator = operator
        return this;
    }

    buildFilterString(searchTerm) {
        if (searchTerm === null || searchTerm === undefined || searchTerm.length === 0) {
            return null;
        }

        const predicates = []

        for (let t of this.startMatch) {
            predicates.push(`startswith(tolower(${t}), tolower('${searchTerm}'))`)
        }
        for (let t of this.endMatch) {
            predicates.push(`endswith(tolower(${t}), tolower('${searchTerm}'))`)
        }
        for (let t of this.eqMatch) {
            predicates.push(`tolower(${t}) eq tolower('${searchTerm}')`)
        }
        for (let t of this.containsMatch) {
            predicates.push(`contains(${t}, '${searchTerm}')`)
        }

        let filterPredicate = `$filter=${predicates.join(` ${this.operator} `)}`

        let filterString = filterPredicate

        if (this.select && this.select.length > 0) {
            filterString = `${filterString}&$select=${this.select.join(',')}`
        }

        return filterString
    }

    buildFilterStringWithList(list) {
        // Warning! This gets out of hand really quickly and can build a gnarly query string
        // that straig up wont work. Be sure you're using it correctly.
        if (list === null || list === undefined) {
            return null;
        }

        if (!Array.isArray(list)) {
            return this.buildFilterString(list);
        }

        const predicates = []

        for (let term of list) {
            for (let t of this.startMatch) {
                predicates.push(`startswith(tolower(${t}), tolower('${term}'))`)
            }
            for (let t of this.endMatch) {
                predicates.push(`endswith(tolower(${t}), tolower('${term}'))`)
            }
            for (let t of this.eqMatch) {
                predicates.push(`tolower(${t}) eq tolower('${term}')`)
            }
            for (let t of this.containsMatch) {
                predicates.push(`contains(${t}, '${term}')`)
            }
        }
        
        let filterPredicate = `$filter=${predicates.join(` ${this.operator} `)}`

        let filterString = filterPredicate

        if (this.select && this.select.length > 0) {
            filterString = `${filterString}&$select=${this.select.join(',')}`
        }

        return filterString
    }
}


const { createApp } = Vue


//const app = createApp(appDefinition);
//app.config.compilerOptions.isCustomElement = (tag) => tag.includes('-')
//app.component('SearchTable', SearchTable);
//app.mount('#app');